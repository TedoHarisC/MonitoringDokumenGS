/* settings-audit-log.js
   Settings > Audit Log page
   - GET /api/audit-logs
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

    function swalError(title, err) {
        const message =
            (err && (err.message || err.title)) ||
            (err && err.errors && Array.isArray(err.errors) ? err.errors.join(', ') : null) ||
            'Something went wrong.'
        Swal.fire(title || 'Error', message, 'error')
    }

    function safePrettyJson(value) {
        if (!value) return ''
        const text = String(value)
        try {
            const obj = JSON.parse(text)
            return JSON.stringify(obj, null, 2)
        } catch {
            return text
        }
    }

    function showModal(modalId) {
        const el = document.getElementById(modalId)
        if (!el) return

        // Some admin templates apply transforms/filters to content wrappers.
        // If the modal lives inside those wrappers, it can render behind the backdrop or look "covered".
        // Moving it under <body> avoids stacking-context issues.
        if (el.parentElement !== document.body) {
            document.body.appendChild(el)
        }
        const modal = new bootstrap.Modal(el)
        modal.show()
    }

    async function copyToClipboard(text) {
        try {
            await navigator.clipboard.writeText(text || '')
            await Swal.fire('Copied', 'Copied to clipboard.', 'success')
        } catch {
            await Swal.fire('Info', 'Copy failed (browser permissions).', 'info')
        }
    }

    const api = {
        list: '/api/audit-logs?page=1&pageSize=200'
    }

    let table
    let lastOld = ''
    let lastNew = ''

    function initTable() {
        table = $('#auditTable').DataTable({
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
                url: api.list,
                dataSrc: function (json) {
                    return normalizeToArray(json)
                }
            },
            columns: [
                {
                    data: 'createdAt',
                    render: function (v) {
                        if (!v) return '-'
                        const d = new Date(v)
                        if (Number.isNaN(d.getTime())) return String(v)
                        return d.toLocaleString()
                    }
                },
                {
                    data: null,
                    render: function (data, type, row) {
                        const name = row.username ? row.username : ''
                        const id = row.userId && row.userId !== '00000000-0000-0000-0000-000000000000' ? row.userId : ''
                        if (!name && !id) return '<span class="text-muted">-</span>'
                        return `${name}${name && id ? '<br>' : ''}<small class="text-muted">${id}</small>`
                    }
                },
                {
                    data: 'entityName',
                    render: function (v) {
                        return v ? String(v) : '<span class="text-muted">-</span>'
                    }
                },
                {
                    data: 'entityId',
                    render: function (v) {
                        return v ? `<code>${String(v)}</code>` : '<span class="text-muted">-</span>'
                    }
                },
                {
                    data: null,
                    orderable: false,
                    searchable: false,
                    className: 'text-center text-nowrap dt-actions',
                    render: function (data, type, row) {
                        const id = row.auditLogId
                        const oldData = row.oldData || ''
                        const newData = row.newData || ''
                        return `
                            <button type="button" class="btn btn-sm btn-light btn-audit-view" 
                                data-id="${id}" 
                                data-old="${encodeURIComponent(oldData)}" 
                                data-new="${encodeURIComponent(newData)}">
                                <i class="feather-eye me-1"></i> View
                            </button>
                        `
                    }
                }
            ],
            order: [[0, 'desc']],
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
                infoEmpty: 'No logs found',
                zeroRecords: 'No matching logs',
                paginate: {
                    previous: '<i class="feather-chevron-left"></i>',
                    next: '<i class="feather-chevron-right"></i>'
                }
            }
        })

        $('#btnRefreshAudit').on('click', function () {
            if (table) table.ajax.reload(null, false)
        })

        $('#auditTable').on('click', '.btn-audit-view', function () {
            const oldData = decodeURIComponent($(this).data('old') || '')
            const newData = decodeURIComponent($(this).data('new') || '')

            lastOld = safePrettyJson(oldData)
            lastNew = safePrettyJson(newData)

            $('#auditOld').text(lastOld || '(empty)')
            $('#auditNew').text(lastNew || '(empty)')

            showModal('auditModal')
        })

        $('#btnCopyOld').on('click', function () {
            copyToClipboard(lastOld)
        })

        $('#btnCopyNew').on('click', function () {
            copyToClipboard(lastNew)
        })
    }

    $(function () {
        initTable()
    })
})(jQuery)
