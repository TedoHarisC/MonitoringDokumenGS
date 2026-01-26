/* transaction-invoice.js
   Transactions > Invoice page
   Uses API endpoints:
   - /api/invoices
   - /api/vendors?page=1&pageSize=2000
   - /api/invoice-progress-statuses?page=1&pageSize=2000
*/

;(function ($) {
    'use strict'

    const apis = {
        invoices: '/api/invoices',
        vendors: '/api/vendors?page=1&pageSize=2000',
        progressStatuses: '/api/invoice-progress-statuses?page=1&pageSize=2000',
        attachments: '/api/attachments'
    }

    let table
    let cachedVendors = []
    let cachedStatuses = []
    let pendingFiles = []

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

    function escapeHtml(value) {
        if (value === null || value === undefined) return ''
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;')
    }

    function toGuidString(value) {
        if (!value) return ''
        return String(value)
    }

    function toInt(value) {
        const n = Number(value)
        return Number.isFinite(n) ? n : 0
    }

    function formatMoney(value) {
        const n = Number(value)
        if (!Number.isFinite(n)) return '-'
        return n.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })
    }

    function formatDate(value) {
        if (!value) return ''
        const d = new Date(value)
        if (isNaN(d.getTime())) return String(value)
        return d.toLocaleString()
    }

    function currentUserId() {
        const el = document.getElementById('currentUserId')
        const v = el ? String(el.value || '').trim() : ''
        return v
    }

    function vendorNameById(id) {
        const gid = toGuidString(id)
        if (!gid) return null
        const found = cachedVendors.find(v => String(v.vendorId || v.VendorId) === gid)
        if (!found) return null
        return String(found.vendorName || found.VendorName || '').trim() || null
    }

    function statusNameById(id) {
        const sid = toInt(id)
        if (!sid) return null
        const found = cachedStatuses.find(s => toInt(s.progressStatusId || s.ProgressStatusId) === sid)
        if (!found) return null
        return String(found.name || found.Name || found.code || found.Code || '').trim() || null
    }

    function setSelectOptions(selectEl, options, placeholder) {
        const ph = placeholder ? `<option value="">${escapeHtml(placeholder)}</option>` : ''
        selectEl.innerHTML = ph + options.join('')
    }

    async function loadVendors() {
        const result = await fetchJson(apis.vendors)
        cachedVendors = normalizeToArray(result)
        return cachedVendors
    }

    async function loadStatuses() {
        const result = await fetchJson(apis.progressStatuses)
        cachedStatuses = normalizeToArray(result)
        return cachedStatuses
    }

    function populateFormSelects() {
        const vendorSelect = document.getElementById('vendorId')
        const statusSelect = document.getElementById('progressStatusId')

        const vendorOptions = cachedVendors.map(v => {
            const id = String(v.vendorId || v.VendorId || '')
            const name = String(v.vendorName || v.VendorName || id)
            return `<option value="${escapeHtml(id)}">${escapeHtml(name)}</option>`
        })

        const statusOptions = cachedStatuses.map(s => {
            const id = String(s.progressStatusId || s.ProgressStatusId || '')
            const label = `${String(s.code || s.Code || '')} — ${String(s.name || s.Name || '')}`.trim()
            return `<option value="${escapeHtml(id)}">${escapeHtml(label)}</option>`
        })

        setSelectOptions(vendorSelect, vendorOptions, '-- Select Vendor --')
        setSelectOptions(statusSelect, statusOptions, '-- Select Status --')
    }

    function initTable() {
        table = $('#invoicesTable').DataTable({
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
                url: apis.invoices,
                dataSrc: function (json) {
                    return normalizeToArray(json)
                }
            },
            columns: [
                { data: 'invoiceNumber' },
                {
                    data: 'vendorId',
                    render: function (data) {
                        return escapeHtml(vendorNameById(data) || data || '')
                    }
                },
                {
                    data: 'progressStatusId',
                    render: function (data) {
                        return escapeHtml(statusNameById(data) || data || '')
                    }
                },
                {
                    data: 'invoiceAmount',
                    className: 'text-end',
                    render: function (data) {
                        return escapeHtml(formatMoney(data))
                    }
                },
                {
                    data: 'taxAmount',
                    className: 'text-end',
                    render: function (data) {
                        return escapeHtml(formatMoney(data))
                    }
                },
                {
                    data: 'createdAt',
                    render: function (data) {
                        return escapeHtml(formatDate(data))
                    }
                },
                {
                    data: null,
                    orderable: false,
                    searchable: false,
                    render: function (data, type, row) {
                        const id = row.invoiceId || row.InvoiceId
                        return `
                            <div class="hstack gap-1 justify-content-center flex-nowrap">
                                <button type="button" class="btn btn-sm btn-light-brand btn-edit" data-id="${escapeHtml(id)}">
                                    <i class="feather-edit-2 me-1"></i> Edit
                                </button>
                                <button type="button" class="btn btn-sm btn-light-danger btn-delete" data-id="${escapeHtml(id)}">
                                    <i class="feather-trash-2 me-1"></i> Delete
                                </button>
                            </div>
                        `
                    }
                }
            ],
            pageLength: 10,
            lengthMenu: [10, 20, 50, 100, 200],
            pagingType: 'simple_numbers',
            scrollX: true,
            autoWidth: false,
            language: {
                search: '',
                searchPlaceholder: 'Search invoices... ',
                lengthMenu: '_MENU_ / page',
                info: 'Showing _START_ to _END_ of _TOTAL_ invoices',
                infoEmpty: 'No invoices found',
                zeroRecords: 'No matching invoices',
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
                    width: 170,
                    createdCell: function (td) {
                        td.classList.add('dt-actions')
                    }
                }
            ]
        })
    }

    function clearForm() {
        $('#invoiceId').val('')
        $('#vendorId').val('')
        $('#progressStatusId').val('')
        $('#invoiceNumber').val('')
        $('#invoiceAmount').val('')
        $('#taxAmount').val('')
    }

    function openCreate() {
        portalModalToBody('invoiceModal')
        clearForm()
        $('#invoiceModalLabel').text('Create Invoice')
        $('#attachmentsSection').show()
        $('#attachmentsList').html('<div class="alert alert-info small">Files will be uploaded after saving the invoice</div>')
        pendingFiles = []
        populateFormSelects()
        showModal('invoiceModal')
    }

    async function openEdit(id) {
        portalModalToBody('invoiceModal')
        const data = await fetchJson(`${apis.invoices}/${id}`)
        $('#invoiceModalLabel').text('Edit Invoice')
        $('#invoiceId').val(data.invoiceId)
        populateFormSelects()
        $('#vendorId').val(String(data.vendorId || ''))
        $('#progressStatusId').val(String(data.progressStatusId || ''))
        $('#invoiceNumber').val(data.invoiceNumber || '')
        $('#invoiceAmount').val(data.invoiceAmount ?? '')
        
        // Show attachments section and load files
        $('#attachmentsSection').show()
        await loadAttachments(data.invoiceId)
        
        $('#taxAmount').val(data.taxAmount ?? '')
        showModal('invoiceModal')
    }

    async function saveInvoice(e) {
        e.preventDefault()

        const id = String($('#invoiceId').val() || '').trim()
        const uid = currentUserId()

        const payload = {
            invoiceId: id || undefined,
            vendorId: String($('#vendorId').val() || ''),
            progressStatusId: toInt($('#progressStatusId').val()),
            invoiceNumber: String($('#invoiceNumber').val() || '').trim(),
            invoiceAmount: Number($('#invoiceAmount').val() || 0),
            taxAmount: Number($('#taxAmount').val() || 0),
            createdByUserId: uid || undefined,
            createdBy: uid || undefined,
            updatedBy: uid || undefined
        }

        if (!payload.vendorId) {
            return Swal.fire('Validation', 'Vendor is required.', 'warning')
        }
        if (!payload.progressStatusId) {
            return Swal.fire('Validation', 'Progress status is required.', 'warning')
        }
        if (!payload.invoiceNumber) {
            return Swal.fire('Validation', 'Invoice number is required.', 'warning')
        }

        const method = id ? 'PUT' : 'POST'
        const url = id ? `${apis.invoices}/${id}` : apis.invoices

        try {
            const result = await fetchJson(url, {
                method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })

            // If creating new invoice and have pending files, upload them
            if (!id && pendingFiles.length > 0 && result.invoiceId) {
                await uploadPendingFiles(result.invoiceId)
            }

            hideModal('invoiceModal')
            if (table) table.ajax.reload(null, false)
            Swal.fire('Saved', 'Invoice saved successfully', 'success')
        } catch (err) {
            const message = (err && (err.message || err.title)) || 'Unable to save invoice.'
            Swal.fire('Error', message, 'error')
        }
    }

    async function deleteInvoice(id) {
        const res = await Swal.fire({
            title: 'Delete invoice?',
            text: 'This action cannot be undone.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Delete',
            cancelButtonText: 'Cancel'
        })

        if (!res.isConfirmed) return

        try {
            await fetchJson(`${apis.invoices}/${id}`, { method: 'DELETE' })
            if (table) table.ajax.reload(null, false)
            Swal.fire('Deleted', 'Invoice deleted', 'success')
        } catch (err) {
            const message = (err && (err.message || err.title)) || 'Unable to delete invoice.'
            Swal.fire('Error', message, 'error')
        }
    }

    async function loadAttachments(invoiceId) {
        try {
            const attachments = await fetchJson(`${apis.attachments}/by-reference/${invoiceId}`)
            const list = $('#attachmentsList')
            list.empty()

            if (!attachments || attachments.length === 0) {
                list.html('<div class="text-muted text-center py-3">No attachments yet</div>')
                return
            }

            attachments.forEach(att => {
                const sizeKB = ((att.fileSize || 0) / 1024).toFixed(1)
                const item = $(`
                    <div class="list-group-item d-flex justify-content-between align-items-center">
                        <div>
                            <i class="feather-file me-2"></i>
                            <span>${escapeHtml(att.fileName)}</span>
                            <small class="text-muted ms-2">(${sizeKB} KB)</small>
                        </div>
                        <button type="button" class="btn btn-sm btn-light-danger btn-delete-attachment" data-id="${att.attachmentId}">
                            <i class="feather-trash-2"></i>
                        </button>
                    </div>
                `)
                list.append(item)
            })
        } catch (err) {
            console.error('Failed to load attachments:', err)
        }
    }

    async function uploadFile(file, invoiceId) {
        // If no invoiceId yet (create mode), add to pending list
        if (!invoiceId) {
            pendingFiles.push(file)
            updatePendingFilesList()
            $('#fileUpload').val('') // Clear input for next file
            return
        }

        const formData = new FormData()
        formData.append('file', file)
        formData.append('module', 'Invoices')
        formData.append('attachmentTypeId', '1') // Default type, adjust as needed
        formData.append('referenceId', invoiceId)

        try {
            const res = await fetch(`${apis.attachments}/upload`, {
                method: 'POST',
                body: formData
            })

            if (!res.ok) {
                const err = await res.json().catch(() => ({ message: 'Upload failed' }))
                throw err
            }

            const result = await res.json()
            await loadAttachments(invoiceId)
            $('#fileUpload').val('') // Clear input for next file
        } catch (err) {
            const message = (err && (err.message || err.title)) || 'Upload failed.'
            Swal.fire('Error', message, 'error')
        }
    }

    function updatePendingFilesList() {
        const $list = $('#attachmentsList')
        $list.empty()
        
        if (pendingFiles.length === 0) {
            $list.html('<div class="alert alert-info small">Files will be uploaded after saving the invoice</div>')
            return
        }

        $list.append('<div class="alert alert-info small mb-2">Files to upload after save:</div>')
        pendingFiles.forEach((file, index) => {
            const sizeKb = (file.size / 1024).toFixed(2)
            $list.append(`
                <div class="list-group-item d-flex justify-content-between align-items-center">
                    <div>
                        <strong>${file.name}</strong>
                        <span class="text-muted small ms-2">(${sizeKb} KB)</span>
                    </div>
                    <button type="button" class="btn btn-sm btn-danger btn-remove-pending" data-index="${index}">
                        <i class="bi bi-x-lg"></i>
                    </button>
                </div>
            `)
        })
    }

    async function uploadPendingFiles(invoiceId) {
        if (pendingFiles.length === 0) return

        for (const file of pendingFiles) {
            await uploadFile(file, invoiceId)
        }
        pendingFiles = []
    }

    async function deleteAttachment(attachmentId, invoiceId) {
        const res = await Swal.fire({
            title: 'Delete attachment?',
            text: 'This action cannot be undone.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Delete',
            cancelButtonText: 'Cancel'
        })

        if (!res.isConfirmed) return

        try {
            await fetchJson(`${apis.attachments}/${attachmentId}`, { method: 'DELETE' })
            await loadAttachments(invoiceId)
            Swal.fire('Deleted', 'Attachment deleted', 'success')
        } catch (err) {
            const message = (err && (err.message || err.title)) || 'Delete failed.'
            Swal.fire('Error', message, 'error')
        }
    }

    $(async function () {
        portalModalToBody('invoiceModal')

        try {
            await Promise.all([loadVendors(), loadStatuses()])
        } catch {
            // still allow page to load; table will show IDs
        }

        initTable()

        $('#btnCreateInvoice').on('click', openCreate)

        $('#invoicesTable').on('click', '.btn-edit', function () {
            const id = $(this).data('id')
            openEdit(id).catch(() => Swal.fire('Error', 'Unable to load invoice', 'error'))
        })

        $('#invoicesTable').on('click', '.btn-delete', function () {
            const id = $(this).data('id')
            deleteInvoice(id)
        })

        $('#invoiceForm').on('submit', saveInvoice)

        // File upload handler
        $('#fileUpload').on('change', function (e) {
            const file = e.target.files[0]
            if (!file) return

            const invoiceId = $('#invoiceId').val()
            uploadFile(file, invoiceId)
        })

        // Delete attachment handler
        $(document).on('click', '.btn-delete-attachment', function () {
            const attachmentId = $(this).data('id')
            const invoiceId = $('#invoiceId').val()
            deleteAttachment(attachmentId, invoiceId)
        })

        // Remove pending file handler
        $(document).on('click', '.btn-remove-pending', function () {
            const index = $(this).data('index')
            pendingFiles.splice(index, 1)
            updatePendingFilesList()
        })
    })
})(jQuery)
