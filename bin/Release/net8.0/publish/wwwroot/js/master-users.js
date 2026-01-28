/* master-users.js
   Master Users page (Master/Users)
   Uses API endpoints:
   - /api/users/with-roles
   - /api/users/admin-create
   - /api/users/admin-update/{id}
   - /api/users/{id} (DELETE)
   - /api/roles
   - /api/vendors
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
        const res = await fetch(url, options)
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
                    width: 190,
                    createdCell: function (td) {
                        td.classList.add('dt-actions')
                    }
                }
            ]
        })
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

    const apis = {
        usersWithRoles: '/api/users/with-roles',
        adminCreate: '/api/users/admin-create',
        adminUpdate: '/api/users/admin-update',
        users: '/api/users',
        roles: '/api/roles',
        vendors: '/api/vendors?page=1&pageSize=2000'
    }

    let usersTable
    let cachedRoles = []
    let cachedVendors = []

    async function loadRoles() {
        cachedRoles = normalizeToArray(await fetchJson(apis.roles))
        return cachedRoles
    }

    async function loadVendors() {
        const result = await fetchJson(apis.vendors)
        cachedVendors = normalizeToArray(result)
        return cachedVendors
    }

    function roleNameFromRow(row) {
        const rs = Array.isArray(row.roles) ? row.roles : []
        if (rs.length === 0) return '<span class="text-muted">-</span>'
        // single-select in UI; show first
        const r = rs[0]
        return `<span class="badge bg-light text-dark border">${escapeHtml(r.code || r.name || '')}</span>`
    }

    function setSelectOptions(selectEl, options, placeholder) {
        const ph = placeholder ? `<option value="">${escapeHtml(placeholder)}</option>` : ''
        selectEl.innerHTML = ph + options.join('')
    }

    function openUserModal(mode, row) {
        portalModalToBody('userModal')

        const isEdit = mode === 'edit'
        $('#userModalLabel').text(isEdit ? 'Edit User' : 'Create User')
        $('#passwordHint').text(isEdit ? 'Optional for edit (leave blank to keep current).' : 'Required for create.')

        $('#userId').val(isEdit ? row.userId : '')
        $('#username').val(isEdit ? row.username : '')
        $('#email').val(isEdit ? row.email : '')
        $('#password').val('')
        $('#isActive').prop('checked', isEdit ? !!row.isActive : true)

        const vendorSelect = document.getElementById('vendorId')
        const roleSelect = document.getElementById('roleId')

        const vendorOptions = cachedVendors.map(v => `<option value="${v.vendorId}">${escapeHtml(v.vendorName)}</option>`)
        setSelectOptions(vendorSelect, vendorOptions, '-- None --')

        const roleOptions = cachedRoles.map(r => `<option value="${r.roleId}">${escapeHtml(r.code)} — ${escapeHtml(r.name || '')}</option>`)
        setSelectOptions(roleSelect, roleOptions, '-- Select Role --')

        if (isEdit) {
            const vid = row.vendorId && row.vendorId !== '00000000-0000-0000-0000-000000000000' ? row.vendorId : ''
            $('#vendorId').val(vid)
            const rs = Array.isArray(row.roles) ? row.roles : []
            $('#roleId').val(rs.length ? rs[0].roleId : '')
        } else {
            $('#vendorId').val('')
            $('#roleId').val('')
        }

        showModal('userModal')
    }

    async function refreshUsers() {
        if (usersTable) usersTable.ajax.reload(null, false)
    }

    function wireTable() {
        usersTable = initTable('#usersTable', apis.usersWithRoles, [
            { data: 'username' },
            { data: 'email' },
            { data: 'vendorName' },
            {
                data: null,
                render: function (data, type, row) {
                    return roleNameFromRow(row)
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
                            <button type="button" class="btn btn-sm btn-light-brand btn-user-edit" data-id="${row.userId}">
                                <i class="feather-edit-2 me-1"></i> Edit
                            </button>
                            <button type="button" class="btn btn-sm btn-light-danger btn-user-delete" data-id="${row.userId}" data-username="${escapeHtml(row.username)}">
                                <i class="feather-trash-2 me-1"></i> Delete
                            </button>
                        </div>
                    `
                }
            }
        ])

        $('#btnCreateUser').on('click', function () {
            openUserModal('create', null)
        })

        $('#usersTable').on('click', '.btn-user-edit', async function () {
            const id = $(this).data('id')
            try {
                // fetch from table cache if available
                const row = usersTable
                    .rows()
                    .data()
                    .toArray()
                    .find(x => x.userId === id)

                if (!row) {
                    await Swal.fire('Info', 'Please refresh the table and try again.', 'info')
                    return
                }

                openUserModal('edit', row)
            } catch (err) {
                swalError('Error', err)
            }
        })

        $('#usersTable').on('click', '.btn-user-delete', function () {
            const id = $(this).data('id')
            const username = $(this).data('username')
            confirmDelete(`user ${username}`).then(async result => {
                if (!result.isConfirmed) return
                try {
                    await fetchJson(`${apis.users}/${id}`, { method: 'DELETE' })
                    await refreshUsers()
                } catch (err) {
                    swalError('Error', err)
                }
            })
        })

        $('#userForm').on('submit', async function (e) {
            e.preventDefault()

            const id = ($('#userId').val() || '').trim()
            const isEdit = !!id

            const payload = {
                username: ($('#username').val() || '').trim(),
                email: ($('#email').val() || '').trim(),
                password: ($('#password').val() || '').trim(),
                vendorId: ($('#vendorId').val() || '').trim() || null,
                isActive: $('#isActive').is(':checked'),
                roleId: Number($('#roleId').val() || 0)
            }

            try {
                if (!payload.username) {
                    await Swal.fire('Validation', 'Username is required.', 'warning')
                    return
                }
                if (!payload.email) {
                    await Swal.fire('Validation', 'Email is required.', 'warning')
                    return
                }
                if (!payload.roleId) {
                    await Swal.fire('Validation', 'Role is required.', 'warning')
                    return
                }
                if (!isEdit && !payload.password) {
                    await Swal.fire('Validation', 'Password is required for create.', 'warning')
                    return
                }

                if (!isEdit) {
                    await fetchJson(apis.adminCreate, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(payload)
                    })
                } else {
                    // password optional for edit
                    const updatePayload = {
                        username: payload.username,
                        email: payload.email,
                        password: payload.password || null,
                        vendorId: payload.vendorId,
                        isActive: payload.isActive,
                        roleId: payload.roleId
                    }

                    await fetchJson(`${apis.adminUpdate}/${id}`, {
                        method: 'PUT',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(updatePayload)
                    })
                }

                hideModal('userModal')
                await refreshUsers()
            } catch (err) {
                swalError('Error', err)
            }
        })
    }

    $(function () {
        portalModalToBody('userModal')

        Promise.all([loadRoles(), loadVendors()])
            .then(() => {
                wireTable()
            })
            .catch(err => swalError('Error', err))
    })
})(jQuery)
