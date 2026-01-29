/* settings-admin-users.js
   Admin Users page (Settings/AdminUsers)
   Uses API endpoints:
   - /api/roles (CRUD)
   - /api/roles/assign?userId=&roleId=
   - /api/roles/remove?userId=&roleId=
   - /api/roles/user/{userId}
   - /api/roles/admin-users
   - /api/users
*/

;(function ($) {
    'use strict'

    function normalizeToArray(json) {
        if (!json) return []
        if (Array.isArray(json)) return json
        if (Array.isArray(json.items)) return json.items
        if (Array.isArray(json.data)) return json.data
        return []
    }

    function portalModalToBody(modalId) {
        const el = document.getElementById(modalId)
        if (!el) return
        if (el.parentElement !== document.body) document.body.appendChild(el)
    }

    function showModal(modalId) {
        const el = document.getElementById(modalId)
        if (!el) return
        const modal = new bootstrap.Modal(el)
        modal.show()
    }

    function hideModal(modalId) {
        const el = document.getElementById(modalId)
        if (!el) return
        const modal = bootstrap.Modal.getInstance(el)
        if (modal) modal.hide()
    }

    function swalError(title, err) {
        const message =
            (err && (err.message || err.title)) ||
            (err && err.errors && Array.isArray(err.errors) ? err.errors.join(', ') : null) ||
            'Something went wrong.'
        Swal.fire(title || 'Error', message, 'error')
    }

    function confirmDelete(label) {
        return Swal.fire({
            title: `Delete ${label}?`,
            text: 'This action cannot be undone.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Delete',
            cancelButtonText: 'Cancel'
        })
    }

    async function fetchJson(url, options) {
        const res = await authFetch(url, options)
        if (res.status === 204) return null
        if (!res.ok) {
            let body = null
            try {
                body = await res.json()
            } catch {
                body = { message: `Request failed (${res.status})` }
            }
            throw body
        }
        return await res.json()
    }

    function initTable(tableSelector, ajaxUrl, columns) {
        return $(tableSelector).DataTable({
            dom:
                "<'row align-items-center g-2 mb-3'" +
                "<'col-sm-12 col-md-6' l>" +
                "<'col-sm-12 col-md-6 d-flex justify-content-md-end' f>" +
                ">" +
                "<'row'<'col-12'tr>>" +
                "<'row align-items-center g-2 mt-3'" +
                "<'col-sm-12 col-md-5' i>" +
                "<'col-sm-12 col-md-7 d-flex justify-content-md-end' p>" +
                ">",
            ajax: {
                url: ajaxUrl,
                dataSrc: function (json) {
                    return normalizeToArray(json)
                }
            },
            columns,
            pageLength: 10,
            lengthMenu: [10, 20, 50, 100, 200],
            pagingType: 'simple_numbers',
            scrollX: true,
            autoWidth: false,
            language: {
                search: '',
                searchPlaceholder: 'Search... ',
                lengthMenu: '_MENU_ / page',
                info: 'Showing _START_ to _END_ of _TOTAL_ items',
                infoEmpty: 'No items found',
                zeroRecords: 'No matching items',
                paginate: {
                    previous: '<i class="feather-chevron-left"></i>',
                    next: '<i class="feather-chevron-right"></i>'
                }
            },
            columnDefs: [
                { targets: '_all', className: 'text-nowrap' },
                {
                    targets: -1,
                    className: 'text-center text-nowrap',
                    width: 180,
                    createdCell: function (td) {
                        td.classList.add('dt-actions')
                    }
                }
            ]
        })
    }

    const apis = {
        roles: '/api/roles',
        users: '/api/users'
    }

    const ADMIN_ROLE_CODES = ['ADMIN', 'SUPER_ADMIN']

    let adminUsersTable
    let rolesTable

    function rolesToBadgeHtml(roles) {
        const rs = Array.isArray(roles) ? roles : []
        if (rs.length === 0) return '<span class="text-muted">-</span>'
        return rs
            .map(r => `<span class="badge bg-light text-dark border me-1">${escapeHtml(r.code || r.name || '')}</span>`)
            .join('')
    }

    function escapeHtml(value) {
        if (value === null || value === undefined) return ''
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;')
    }

    async function loadAllRoles() {
        const roles = await fetchJson(apis.roles)
        return normalizeToArray(roles)
    }

    async function loadAllUsers() {
        const users = await fetchJson(apis.users)
        return normalizeToArray(users)
    }

    function renderRoleCheckboxList(containerEl, roles, checkedRoleIds) {
        const checked = new Set((checkedRoleIds || []).map(x => Number(x)))
        const html = roles
            .map(r => {
                const id = Number(r.roleId)
                const isChecked = checked.has(id)
                return `
                    <div class="form-check">
                        <input class="form-check-input role-check" type="checkbox" value="${id}" id="role_${id}" ${isChecked ? 'checked' : ''}>
                        <label class="form-check-label" for="role_${id}">
                            <span class="fw-semibold">${escapeHtml(r.code)}</span>
                            <span class="text-muted">— ${escapeHtml(r.name || '')}</span>
                        </label>
                    </div>
                `
            })
            .join('')

        containerEl.innerHTML = html || '<span class="text-muted">No roles.</span>'
    }

    async function refreshAdminUsers() {
        if (adminUsersTable) adminUsersTable.ajax.reload(null, false)
    }

    async function refreshRoles() {
        if (rolesTable) rolesTable.ajax.reload(null, false)
    }

    async function openManageRolesModal(userId, username) {
        portalModalToBody('userRolesModal')

        $('#manageUserId').val(userId)
        $('#manageUserTitle').text(`User: ${username}`)

        try {
            const [allRoles, userRoles] = await Promise.all([
                loadAllRoles(),
                fetchJson(`${apis.roles}/user/${userId}`)
            ])

            const currentRoleIds = normalizeToArray(userRoles).map(r => Number(r.roleId))
            const container = document.getElementById('manageRoleList')
            renderRoleCheckboxList(container, allRoles, currentRoleIds)
            showModal('userRolesModal')
        } catch (err) {
            swalError('Error', err)
        }
    }

    async function saveUserRolesFromModal() {
        const userId = $('#manageUserId').val()
        if (!userId) return

        const checkedIds = Array.from(document.querySelectorAll('#manageRoleList .role-check:checked')).map(x => Number(x.value))

        const currentRoles = await fetchJson(`${apis.roles}/user/${userId}`)
        const currentIds = normalizeToArray(currentRoles).map(r => Number(r.roleId))

        const toAdd = checkedIds.filter(id => !currentIds.includes(id))
        const toRemove = currentIds.filter(id => !checkedIds.includes(id))

        for (const roleId of toAdd) {
            await fetchJson(`${apis.roles}/assign?userId=${encodeURIComponent(userId)}&roleId=${encodeURIComponent(roleId)}`, { method: 'POST' })
        }

        for (const roleId of toRemove) {
            await fetchJson(`${apis.roles}/remove?userId=${encodeURIComponent(userId)}&roleId=${encodeURIComponent(roleId)}`, { method: 'DELETE' })
        }

        hideModal('userRolesModal')
        await refreshAdminUsers()
    }

    async function openAddAdminUserModal() {
        portalModalToBody('addAdminUserModal')

        try {
            const [users, roles] = await Promise.all([loadAllUsers(), loadAllRoles()])

            const userSelect = document.getElementById('addAdminUserSelect')

            userSelect.innerHTML = users
                .filter(u => !u.isDeleted)
                .map(u => `<option value="${u.userId}">${escapeHtml(u.username)} — ${escapeHtml(u.vendorName || '')}</option>`)
                .join('')

            const adminRoles = roles.filter(r => ADMIN_ROLE_CODES.includes((r.code || '').toUpperCase()))
            const roleList = document.getElementById('addAdminRoleList')
            renderRoleCheckboxList(roleList, adminRoles, adminRoles.map(r => r.roleId))

            showModal('addAdminUserModal')
        } catch (err) {
            swalError('Error', err)
        }
    }

    async function saveAddAdminUser() {
        const userId = $('#addAdminUserSelect').val()
        if (!userId) return

        const checkedIds = Array.from(document.querySelectorAll('#addAdminRoleList .role-check:checked')).map(x => Number(x.value))
        if (checkedIds.length === 0) {
            await Swal.fire('Validation', 'Select at least one admin role.', 'warning')
            return
        }

        for (const roleId of checkedIds) {
            try {
                await fetchJson(`${apis.roles}/assign?userId=${encodeURIComponent(userId)}&roleId=${encodeURIComponent(roleId)}`, { method: 'POST' })
            } catch (err) {
                // ignore duplicate assignment but surface others
                if (!err || !String(err.message || '').toLowerCase().includes('already')) throw err
            }
        }

        hideModal('addAdminUserModal')
        await refreshAdminUsers()
    }

    function wireRolesTab() {
        rolesTable = initTable('#rolesTable', apis.roles, [
            { data: 'code' },
            { data: 'name' },
            {
                data: null,
                orderable: false,
                searchable: false,
                render: function (data, type, row) {
                    return `
                        <div class="hstack gap-1 justify-content-center flex-nowrap">
                            <button type="button" class="btn btn-sm btn-light-brand btn-role-edit" data-id="${row.roleId}">
                                <i class="feather-edit-2 me-1"></i> Edit
                            </button>
                            <button type="button" class="btn btn-sm btn-light-danger btn-role-delete" data-id="${row.roleId}" data-code="${escapeHtml(row.code)}">
                                <i class="feather-trash-2 me-1"></i> Delete
                            </button>
                        </div>
                    `
                }
            }
        ])

        $('#btnCreateRole').on('click', function () {
            portalModalToBody('roleModal')
            $('#roleModalLabel').text('Create Role')
            $('#roleId').val('')
            $('#roleCode').val('')
            $('#roleName').val('')
            showModal('roleModal')
        })

        $('#rolesTable').on('click', '.btn-role-edit', function () {
            const id = $(this).data('id')
            fetchJson(`${apis.roles}/${id}`)
                .then(role => {
                    portalModalToBody('roleModal')
                    $('#roleModalLabel').text('Edit Role')
                    $('#roleId').val(role.roleId)
                    $('#roleCode').val(role.code)
                    $('#roleName').val(role.name)
                    showModal('roleModal')
                })
                .catch(err => swalError('Error', err))
        })

        $('#rolesTable').on('click', '.btn-role-delete', function () {
            const id = $(this).data('id')
            const code = $(this).data('code')
            confirmDelete(`role ${code}`).then(async result => {
                if (!result.isConfirmed) return
                try {
                    await fetchJson(`${apis.roles}/${id}`, { method: 'DELETE' })
                    await refreshRoles()
                } catch (err) {
                    swalError('Error', err)
                }
            })
        })

        $('#roleForm').on('submit', async function (e) {
            e.preventDefault()
            const roleId = $('#roleId').val()
            const payload = {
                roleId: roleId ? Number(roleId) : 0,
                code: ($('#roleCode').val() || '').trim(),
                name: ($('#roleName').val() || '').trim()
            }

            try {
                if (!payload.code) {
                    await Swal.fire('Validation', 'Role code is required.', 'warning')
                    return
                }

                if (!roleId) {
                    await fetchJson(apis.roles, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(payload)
                    })
                } else {
                    await fetchJson(`${apis.roles}/${payload.roleId}`, {
                        method: 'PUT',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(payload)
                    })
                }

                hideModal('roleModal')
                await refreshRoles()
                await refreshAdminUsers()
            } catch (err) {
                swalError('Error', err)
            }
        })
    }

    function wireAdminUsersTab() {
        adminUsersTable = initTable('#adminUsersTable', `${apis.roles}/admin-users`, [
            { data: 'username' },
            { data: 'email' },
            {
                data: 'roles',
                render: function (data) {
                    return rolesToBadgeHtml(data)
                }
            },
            {
                data: 'isActive',
                render: function (v) {
                    return v ? '<span class="badge bg-success">Active</span>' : '<span class="badge bg-secondary">Inactive</span>'
                }
            },
            {
                data: null,
                orderable: false,
                searchable: false,
                render: function (data, type, row) {
                    return `
                        <div class="hstack gap-1 justify-content-center flex-nowrap">
                            <button type="button" class="btn btn-sm btn-light-brand btn-manage-roles" data-id="${row.userId}" data-username="${escapeHtml(row.username)}">
                                <i class="feather-shield me-1"></i> Roles
                            </button>
                        </div>
                    `
                }
            }
        ])

        $('#btnAddAdminUser').on('click', function () {
            openAddAdminUserModal()
        })

        $('#adminUsersTable').on('click', '.btn-manage-roles', function () {
            const userId = $(this).data('id')
            const username = $(this).data('username')
            openManageRolesModal(userId, username)
        })

        $('#userRolesForm').on('submit', async function (e) {
            e.preventDefault()
            try {
                await saveUserRolesFromModal()
            } catch (err) {
                swalError('Error', err)
            }
        })

        $('#addAdminUserForm').on('submit', async function (e) {
            e.preventDefault()
            try {
                await saveAddAdminUser()
            } catch (err) {
                swalError('Error', err)
            }
        })
    }

    $(function () {
        portalModalToBody('addAdminUserModal')
        portalModalToBody('userRolesModal')
        portalModalToBody('roleModal')

        wireAdminUsersTab()
        wireRolesTab()

        // Fix header/body misalignment when DataTable is initialized inside a hidden tab
        // (Bootstrap tabs hide panels with display:none, so widths can't be measured correctly).
        $('button[data-bs-toggle="tab"]').on('shown.bs.tab', function (e) {
            const targetId = e && e.target && e.target.id ? e.target.id : ''

            if (targetId === 'tab-roles-btn' && rolesTable) {
                rolesTable.columns.adjust().draw(false)
            }

            if (targetId === 'tab-admin-users-btn' && adminUsersTable) {
                adminUsersTable.columns.adjust().draw(false)
            }
        })
    })
})(jQuery)
