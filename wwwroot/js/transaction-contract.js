;(function ($) {
    'use strict'

    const apis = {
        contracts: '/api/contracts',
        vendors: '/api/vendors?page=1&pageSize=2000',
        contractStatuses: '/api/contract-statuses?page=1&pageSize=2000',
        approvalStatuses: '/api/approval-statuses?page=1&pageSize=2000',
        attachments: '/api/attachments',
        currentUser: '/api/auth/me'
    }

    let table
    let cachedVendors = []
    let cachedContractStatuses = []
    let cachedApprovalStatuses = []
    let pendingFiles = []
    let currentUserVendor = null

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

    function formatDate(value) {
        if (!value) return ''
        const d = new Date(value)
        if (isNaN(d.getTime())) return String(value)
        const yyyy = d.getFullYear()
        const mm = String(d.getMonth() + 1).padStart(2, '0')
        const dd = String(d.getDate()).padStart(2, '0')
        return `${yyyy}-${mm}-${dd}`
    }

    function formatDateTime(value) {
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

    function isCurrentUserAdmin() {
        const el = document.getElementById('currentUserIsAdmin')
        const v = el ? String(el.value || '').trim().toLowerCase() : 'false'
        return v === 'true'
    }

    async function loadCurrentUser() {
        const result = await fetchJson(apis.currentUser, { credentials: 'same-origin' })
        currentUserVendor = {
            vendorId: result.vendorId,
            vendorName: result.vendorName
        }
    }

    function vendorNameById(id) {
        const gid = toGuidString(id)
        if (!gid) return null
        
        // First check current user vendor
        if (currentUserVendor && String(currentUserVendor.vendorId).toLowerCase() === String(gid).toLowerCase().trim()) {
            return currentUserVendor.vendorName
        }
        
        const found = cachedVendors.find(v => String(v.vendorId || v.VendorId).toLowerCase() === gid.toLowerCase())
        if (!found) return null
        return String(found.vendorName || found.VendorName || '').trim() || null
    }

    function contractStatusNameById(id) {
        const sid = toInt(id)
        if (!sid) return null
        const found = cachedContractStatuses.find(s => toInt(s.contractStatusId || s.ContractStatusId) === sid)
        if (!found) return null
        return String(found.name || found.Name || found.code || found.Code || '').trim() || null
    }

    function approvalStatusNameById(id) {
        const sid = toInt(id)
        if (!sid) return null
        const found = cachedApprovalStatuses.find(s => toInt(s.approvalStatusId || s.ApprovalStatusId) === sid)
        if (!found) return null
        return String(found.name || found.Name || found.code || found.Code || '').trim() || null
    }

    function getVendorColor(vendorId) {
        // Array of Bootstrap badge color classes
        const colors = [
            'primary',
            'success',
            'info',
            'warning',
            'danger',
            'secondary',
            'dark'
        ]
        
        // Generate consistent color based on vendor ID
        const id = String(vendorId || '').toLowerCase()
        let hash = 0
        for (let i = 0; i < id.length; i++) {
            hash = id.charCodeAt(i) + ((hash << 5) - hash)
        }
        const index = Math.abs(hash) % colors.length
        return colors[index]
    }

    function getContractStatusColor(statusId) {
        // Map contract status IDs to colors
        const statusColors = {
            1: 'secondary',  // e.g., Draft
            2: 'info',       // e.g., Active
            3: 'success',    // e.g., Completed
            4: 'warning',    // e.g., On Hold
            5: 'danger',     // e.g., Terminated
            6: 'primary'     // e.g., Renewed
        }
        return statusColors[statusId] || 'secondary'
    }

    function getApprovalStatusColor(statusId) {
        // Map approval status IDs to colors
        const statusColors = {
            6: 'warning',  // e.g., Draft
            7: 'secondary',    // e.g., Pending Review
            8: 'info',       // e.g., In Review
            9: 'primary',    // e.g., Approved
            10: 'success',     // e.g., Rejected
        }
        return statusColors[statusId] || 'secondary'
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

    async function loadContractStatuses() {
        const result = await fetchJson(apis.contractStatuses)
        cachedContractStatuses = normalizeToArray(result)
        return cachedContractStatuses
    }

    async function loadApprovalStatuses() {
        const result = await fetchJson(apis.approvalStatuses)
        cachedApprovalStatuses = normalizeToArray(result)
        return cachedApprovalStatuses
    }

    function populateContractStatusDropdown() {
        const $select = $('#contractStatusSelect')
        $select.empty()
        $select.append('<option value="">-- Select Contract Status --</option>')
        
        cachedContractStatuses.forEach(status => {
            const id = status.contractStatusId || status.ContractStatusId
            const name = status.name || status.Name || status.code || status.Code
            $select.append(`<option value="${id}">${escapeHtml(name)}</option>`)
        })
    }

    function populateApprovalStatusDropdown() {
        const $select = $('#approvalStatusSelect')
        $select.empty()
        $select.append('<option value="">-- Select Approval Status --</option>')
        
        cachedApprovalStatuses.forEach(status => {
            const id = status.approvalStatusId || status.ApprovalStatusId
            const name = status.name || status.Name || status.code || status.Code
            $select.append(`<option value="${id}">${escapeHtml(name)}</option>`)
        })
    }

    function populateFormSelects() {
        const vendorSelect = document.getElementById('vendorId')
        const contractStatusSelect = document.getElementById('contractStatusId')
        const approvalStatusSelect = document.getElementById('approvalStatusId')

        const vendorOptions = cachedVendors.map(v => {
            const id = String(v.vendorId || v.VendorId || '')
            const name = String(v.vendorName || v.VendorName || id)
            return `<option value="${escapeHtml(id)}">${escapeHtml(name)}</option>`
        })

        const contractStatusOptions = cachedContractStatuses.map(s => {
            const id = String(s.contractStatusId || s.ContractStatusId || '')
            const label = `${String(s.name || s.Name || '')}`.trim()
            return `<option value="${escapeHtml(id)}">${escapeHtml(label)}</option>`
        })

        const approvalStatusOptions = cachedApprovalStatuses.map(s => {
            const id = String(s.approvalStatusId || s.ApprovalStatusId || '')
            const label = `${String(s.name || s.Name || '')}`.trim()
            return `<option value="${escapeHtml(id)}">${escapeHtml(label)}</option>`
        })

        setSelectOptions(vendorSelect, vendorOptions, '-- Select Vendor --')
        setSelectOptions(contractStatusSelect, contractStatusOptions, '-- Select Contract Status --')
        setSelectOptions(approvalStatusSelect, approvalStatusOptions, '-- Select Approval Status --')
    }

    function initTable() {
        table = $('#contractsTable').DataTable({
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
                url: apis.contracts,
                dataSrc: function (json) {
                    return normalizeToArray(json)
                }
            },
            columns: [
                { data: 'contractNumber' },
                {
                    data: 'vendorId',
                    render: function (data) {
                        const vendorName = vendorNameById(data) || data || 'Unknown'
                        const colorClass = getVendorColor(data)
                        return `<span class="badge bg-${colorClass} bg-soft-${colorClass} text-white" style="font-size: 0.875rem; padding: 0.35rem 0.75rem;">${escapeHtml(vendorName)}</span>`
                    }
                },
                {
                    data: 'contractDescription',
                    render: function (data) {
                        const txt = String(data || '')
                        return txt.length > 50 ? escapeHtml(txt.substring(0, 50)) + '...' : escapeHtml(txt)
                    }
                },
                {
                    data: 'startDate',
                    render: function (data) {
                        return escapeHtml(formatDate(data))
                    }
                },
                {
                    data: 'endDate',
                    render: function (data) {
                        return escapeHtml(formatDate(data))
                    }
                },
                {
                    data: 'contractStatusId',
                    render: function (data) {
                        const statusName = contractStatusNameById(data) || data || 'Unknown'
                        const colorClass = getContractStatusColor(data)
                        return `<span class="fw-bold text-${colorClass}" style="font-size: 0.9rem;">${escapeHtml(statusName)}</span>`
                    }
                },
                {
                    data: 'approvalStatusId',
                    render: function (data) {
                        const statusName = approvalStatusNameById(data) || data || 'Unknown'
                        const colorClass = getApprovalStatusColor(data)
                        return `<span class="badge bg-${colorClass} text-white" style="font-size: 0.875rem; padding: 0.35rem 0.75rem;">${escapeHtml(statusName)}</span>`
                    }
                },
                {
                    data: null,
                    orderable: false,
                    searchable: false,
                    render: function (data, type, row) {
                        const id = row.contractId || row.ContractId
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
                searchPlaceholder: 'Search contracts... ',
                lengthMenu: '_MENU_ / page',
                info: 'Showing _START_ to _END_ of _TOTAL_ contracts',
                infoEmpty: 'No contracts found',
                zeroRecords: 'No matching contracts',
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
        $('#contractId').val('')
        $('#vendorId').val('')
        $('#vendorName').val('')
        $('#contractNumber').val('')
        $('#contractDescription').val('')
        $('#startDate').val('')
        $('#endDate').val('')
        $('#contractStatusId').val('2')
        $('#contractStatusSelect').val('2')
        $('#approvalStatusId').val('6')
        $('#approvalStatusSelect').val('6')
    }

    function openCreate() {
        portalModalToBody('contractModal')
        clearForm()
        $('#contractModalLabel').text('Create Contract')
        
        // Set vendor from current user
        if (currentUserVendor && currentUserVendor.vendorId) {
            $('#vendorId').val(currentUserVendor.vendorId)
            $('#vendorName').val(currentUserVendor.vendorName || '')
        } else {
            Swal.fire('Warning', 'No vendor assigned to your account. Please contact administrator.', 'warning')
            return
        }
        
        // Set default statuses
        $('#contractStatusId').val('2')
        $('#contractStatusSelect').val('2')
        $('#approvalStatusId').val('6')
        $('#approvalStatusSelect').val('6')
        
        // Show/hide status sections based on user role
        if (isCurrentUserAdmin()) {
            populateContractStatusDropdown()
            populateApprovalStatusDropdown()
            $('#contractStatusSection').show()
            $('#approvalStatusSection').show()
        } else {
            $('#contractStatusSection').hide()
            $('#approvalStatusSection').hide()
        }
        
        $('#attachmentsSection').show()
        $('#attachmentsList').html('<div class="alert alert-info small">Files will be uploaded after saving the contract</div>')
        pendingFiles = []
        showModal('contractModal')
    }

    async function openEdit(id) {
        portalModalToBody('contractModal')
        const data = await fetchJson(`${apis.contracts}/${id}`)
        $('#contractModalLabel').text('Edit Contract')
        $('#contractId').val(data.contractId)
        
        // Set vendor
        const vendorName = vendorNameById(data.vendorId) || ''
        $('#vendorId').val(String(data.vendorId || ''))
        $('#vendorName').val(vendorName)
        
        $('#contractNumber').val(data.contractNumber || '')
        $('#contractDescription').val(data.contractDescription || '')
        $('#startDate').val(formatDate(data.startDate))
        $('#endDate').val(formatDate(data.endDate))
        
        // Set status values
        $('#contractStatusId').val(String(data.contractStatusId || ''))
        $('#contractStatusSelect').val(String(data.contractStatusId || ''))
        $('#approvalStatusId').val(String(data.approvalStatusId || ''))
        $('#approvalStatusSelect').val(String(data.approvalStatusId || ''))
        
        // Show/hide status sections based on user role
        if (isCurrentUserAdmin()) {
            populateContractStatusDropdown()
            populateApprovalStatusDropdown()
            $('#contractStatusSection').show()
            $('#contractStatusSelect').val(String(data.contractStatusId || ''))
            $('#approvalStatusSection').show()
            $('#approvalStatusSelect').val(String(data.approvalStatusId || ''))
        } else {
            $('#contractStatusSection').hide()
            $('#approvalStatusSection').hide()
        }
        
        $('#attachmentsSection').show()
        await loadAttachments(data.contractId)
        showModal('contractModal')
    }

    async function saveContract(e) {
        e.preventDefault()

        const id = String($('#contractId').val() || '').trim()
        const uid = currentUserId()
        const vendorId = String($('#vendorId').val() || '').trim()
        const isEdit = !!id

        // Get statuses - from select if admin, otherwise from hidden field
        const contractStatusId = isCurrentUserAdmin() 
            ? toInt($('#contractStatusSelect').val())
            : toInt($('#contractStatusId').val())
        
        const approvalStatusId = isCurrentUserAdmin() 
            ? toInt($('#approvalStatusSelect').val())
            : toInt($('#approvalStatusId').val())

        const payload = {
            contractId: id || undefined,
            vendorId: vendorId,
            contractNumber: String($('#contractNumber').val() || '').trim(),
            contractDescription: String($('#contractDescription').val() || '').trim(),
            startDate: $('#startDate').val(),
            endDate: $('#endDate').val(),
            contractStatusId: contractStatusId,
            approvalStatusId: approvalStatusId,
            createdByUserId: uid || undefined,
            createdBy: uid || undefined,
            updatedBy: uid || undefined
        }

        if (!payload.vendorId) {
            return Swal.fire('Validation', 'Vendor is required.', 'warning')
        }
        if (!payload.contractNumber) {
            return Swal.fire('Validation', 'Contract number is required.', 'warning')
        }
        if (!payload.contractDescription) {
            return Swal.fire('Validation', 'Description is required.', 'warning')
        }
        if (!payload.startDate || !payload.endDate) {
            return Swal.fire('Validation', 'Start and End dates are required.', 'warning')
        }
        if (!payload.contractStatusId) {
            return Swal.fire('Validation', 'Contract status is required.', 'warning')
        }
        if (!payload.approvalStatusId) {
            return Swal.fire('Validation', 'Approval status is required.', 'warning')
        }

        const method = id ? 'PUT' : 'POST'
        const url = id ? `${apis.contracts}/${id}` : apis.contracts

        try {
            const result = await fetchJson(url, {
                method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })

            // If creating new contract and have pending files, upload them
            if (!id && pendingFiles.length > 0 && result.contractId) {
                await uploadPendingFiles(result.contractId)
            }

            hideModal('contractModal')
            if (table) table.ajax.reload(null, false)
            Swal.fire('Saved', 'Contract saved successfully', 'success')
        } catch (err) {
            const message = (err && (err.message || err.title)) || 'Unable to save contract.'
            Swal.fire('Error', message, 'error')
        }
    }

    async function loadAttachments(contractId) {
        try {
            const attachments = await fetchJson(`${apis.attachments}/by-reference/${contractId}`)
            const $list = $('#attachmentsList')
            $list.empty()

            if (!attachments || attachments.length === 0) {
                $list.html('<div class="text-muted text-center py-3">No attachments yet</div>')
                return
            }

            attachments.forEach(att => {
                const sizeKB = ((att.fileSize || 0) / 1024).toFixed(1)
                const fileUrl = `/api/attachments/download/${att.attachmentId}`
                
                const item = $(`
                    <div class="list-group-item d-flex justify-content-between align-items-center">
                        <div class="flex-grow-1" style="cursor: pointer;">
                            <a href="${fileUrl}" target="_blank" class="text-decoration-none text-dark d-flex align-items-center">
                                <i class="feather-file me-2"></i>
                                <span class="text-primary">${escapeHtml(att.fileName)}</span>
                                <small class="text-muted ms-2">(${sizeKB} KB)</small>
                                <i class="feather-external-link ms-2 text-muted" style="font-size: 14px;"></i>
                            </a>
                        </div>
                        <button type="button" class="btn btn-sm btn-light-danger btn-delete-attachment ms-2" data-id="${att.attachmentId}">
                            <i class="feather-trash-2"></i>
                        </button>
                    </div>
                `)
                $list.append(item)
            })
        } catch (err) {
            console.error('Failed to load attachments:', err)
        }
    }

    async function uploadFile(file, contractId) {
        // If no contractId yet (create mode), add to pending list
        if (!contractId) {
            pendingFiles.push(file)
            updatePendingFilesList()
            $('#fileUpload').val('') // Clear input for next file
            return
        }

        const formData = new FormData()
        formData.append('file', file)
        formData.append('module', 'Contracts')
        formData.append('attachmentTypeId', '1')
        formData.append('referenceId', contractId)

        try {
            await autFetch(`${apis.attachments}/upload`, {
                method: 'POST',
                body: formData
            })

            await loadAttachments(contractId)
            $('#fileUpload').val('') // Clear input for next file
        } catch (err) {
            Swal.fire('Error', 'Failed to upload file', 'error')
        }
    }

    function updatePendingFilesList() {
        const $list = $('#attachmentsList')
        $list.empty()
        
        if (pendingFiles.length === 0) {
            $list.html('<div class="alert alert-info small">Files will be uploaded after saving the contract</div>')
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

    async function uploadPendingFiles(contractId) {
        if (pendingFiles.length === 0) return

        for (const file of pendingFiles) {
            await uploadFile(file, contractId)
        }
        pendingFiles = []
    }

    async function deleteAttachment(attachmentId, contractId) {
        const res = await Swal.fire({
            title: 'Delete attachment?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Yes, delete'
        })

        if (!res.isConfirmed) return

        try {
            await authFetch(`${apis.attachments}/${attachmentId}`, { method: 'DELETE' })
            Swal.fire('Deleted', 'Attachment deleted', 'success')
            await loadAttachments(contractId)
        } catch (err) {
            Swal.fire('Error', 'Failed to delete attachment', 'error')
        }
    }

    async function deleteContract(id) {
        const res = await Swal.fire({
            title: 'Delete contract?',
            text: 'This action cannot be undone.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Delete',
            cancelButtonText: 'Cancel'
        })

        if (!res.isConfirmed) return

        try {
            await fetchJson(`${apis.contracts}/${id}`, { method: 'DELETE' })
            if (table) table.ajax.reload(null, false)
            Swal.fire('Deleted', 'Contract deleted', 'success')
        } catch (err) {
            const message = (err && (err.message || err.title)) || 'Unable to delete contract.'
            Swal.fire('Error', message, 'error')
        }
    }

    $(async function () {
        portalModalToBody('contractModal')

        try {
            await Promise.all([loadCurrentUser(), loadVendors(), loadContractStatuses(), loadApprovalStatuses()])
        } catch (err) {
            console.error('Initialization error:', err)
            // still allow page to load; table will show IDs
        }

        initTable()

        $('#btnCreateContract').on('click', openCreate)

        $('#contractsTable').on('click', '.btn-edit', function () {
            const id = $(this).data('id')
            openEdit(id).catch(() => Swal.fire('Error', 'Unable to load contract', 'error'))
        })

        $('#contractsTable').on('click', '.btn-delete', function () {
            const id = $(this).data('id')
            deleteContract(id)
        })

        $('#fileUpload').on('change', function () {
            const file = this.files[0]
            if (file) {
                const contractId = $('#contractId').val()
                uploadFile(file, contractId)
            }
        })

        $(document).on('click', '.btn-delete-attachment', function () {
            const attachmentId = $(this).data('id')
            const contractId = $('#contractId').val()
            deleteAttachment(attachmentId, contractId)
        })

        $(document).on('click', '.btn-remove-pending', function () {
            const index = $(this).data('index')
            pendingFiles.splice(index, 1)
            updatePendingFilesList()
        })

        $('#contractForm').on('submit', saveContract)
    })
})(jQuery)
