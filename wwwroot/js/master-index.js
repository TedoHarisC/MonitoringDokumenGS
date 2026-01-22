/* master-index.js
   One-page CRUD for Master data (except Vendor)
   Uses API endpoints:
   - /api/approval-statuses
   - /api/attachment-types
   - /api/contract-statuses
   - /api/invoice-progress-statuses
   - /api/vendor-categories
*/

;(function ($) {
    'use strict';

    function normalizeToArray(json) {
        if (!json) return [];
        if (Array.isArray(json)) return json;
        if (Array.isArray(json.items)) return json.items;
        if (Array.isArray(json.data)) return json.data;
        return [];
    }

    function portalModalToBody(modalId) {
        const el = document.getElementById(modalId);
        if (!el) return;
        if (el.parentElement !== document.body) document.body.appendChild(el);
    }

    function showModal(modalId) {
        const el = document.getElementById(modalId);
        if (!el) return;
        const modal = new bootstrap.Modal(el);
        modal.show();
    }

    function hideModal(modalId) {
        const el = document.getElementById(modalId);
        if (!el) return;
        const modal = bootstrap.Modal.getInstance(el);
        if (modal) modal.hide();
    }

    function swalError(title, err) {
        const message =
            (err && (err.message || err.title)) ||
            (err && err.errors && Array.isArray(err.errors) ? err.errors.join(', ') : null) ||
            'Something went wrong.';
        Swal.fire(title || 'Error', message, 'error');
    }

    function confirmDelete(label) {
        return Swal.fire({
            title: `Delete ${label}?`,
            text: 'This action cannot be undone.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Delete',
            cancelButtonText: 'Cancel'
        });
    }

    function pad2(n) {
        return String(n).padStart(2, '0');
    }

    function buildTimestampForFilename() {
        const d = new Date();
        const yyyy = d.getFullYear();
        const mm = pad2(d.getMonth() + 1);
        const dd = pad2(d.getDate());
        const hh = pad2(d.getHours());
        const mi = pad2(d.getMinutes());
        return `${yyyy}${mm}${dd}_${hh}${mi}`;
    }

    function downloadBlob(filename, mimeType, content) {
        const blob = content instanceof Blob ? content : new Blob([content], { type: mimeType });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        a.remove();
        URL.revokeObjectURL(url);
    }

    function csvEscape(value) {
        if (value === null || value === undefined) return '';
        const s = String(value);
        const needsQuote = /[\r\n",]/.test(s);
        const escaped = s.replace(/"/g, '""');
        return needsQuote ? `"${escaped}"` : escaped;
    }

    function htmlEscape(value) {
        if (value === null || value === undefined) return '';
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function getFilteredRows(dataTable) {
        if (!dataTable) return [];
        try {
            return dataTable.rows({ search: 'applied' }).data().toArray();
        } catch {
            return [];
        }
    }

    function exportToCsv(dataTable, columns, filenameBase) {
        const rows = getFilteredRows(dataTable);
        const headers = columns.map(c => csvEscape(c.header)).join(',');
        const lines = rows.map(r => columns.map(c => csvEscape(c.value(r))).join(','));
        // Add UTF-8 BOM so Excel opens UTF-8 nicely
        const csv = '\ufeff' + [headers, ...lines].join('\r\n');
        const filename = `${filenameBase}_${buildTimestampForFilename()}.csv`;
        downloadBlob(filename, 'text/csv;charset=utf-8', csv);
    }

    function exportToExcelHtml(dataTable, columns, filenameBase) {
        const rows = getFilteredRows(dataTable);
        const head = `<tr>${columns.map(c => `<th>${htmlEscape(c.header)}</th>`).join('')}</tr>`;
        const body = rows
            .map(r => `<tr>${columns.map(c => `<td>${htmlEscape(c.value(r))}</td>`).join('')}</tr>`)
            .join('');
        const html = `<!doctype html><html><head><meta charset="utf-8"></head><body><table>${head}${body}</table></body></html>`;
        const filename = `${filenameBase}_${buildTimestampForFilename()}.xls`;
        downloadBlob(filename, 'application/vnd.ms-excel;charset=utf-8', html);
    }

    function initSimpleTable(tableSelector, apiBase, columns, options) {
        const table = $(tableSelector).DataTable({
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
                url: `${apiBase}?page=1&pageSize=2000`,
                dataSrc: function (json) {
                    return normalizeToArray(json);
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
                searchPlaceholder: (options && options.searchPlaceholder) || 'Search... ',
                lengthMenu: '_MENU_ / page',
                info: (options && options.infoText) || 'Showing _START_ to _END_ of _TOTAL_ items',
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
                    width: 170,
                    createdCell: function (td) {
                        td.classList.add('dt-actions');
                    }
                }
            ]
        });

        return table;
    }

    const apis = {
        approval: '/api/approval-statuses',
        attachment: '/api/attachment-types',
        contract: '/api/contract-statuses',
        invoiceProgress: '/api/invoice-progress-statuses',
        vendorCategory: '/api/vendor-categories'
    };

    let approvalTable;
    let attachmentTable;
    let contractTable;
    let invoiceProgressTable;
    let vendorCategoryTable;

    function wireApproval() {
        approvalTable = initSimpleTable(
            '#approvalStatusesTable',
            apis.approval,
            [
                { data: 'code' },
                { data: 'name' },
                {
                    data: null,
                    orderable: false,
                    searchable: false,
                    render: function (data, type, row) {
                        return `
                            <div class="hstack gap-1 justify-content-center flex-nowrap">
                                <button type="button" class="btn btn-sm btn-light-brand btn-approval-edit" data-id="${row.approvalStatusId}">
                                    <i class="feather-edit-2 me-1"></i> Edit
                                </button>
                                <button type="button" class="btn btn-sm btn-light-danger btn-approval-delete" data-id="${row.approvalStatusId}">
                                    <i class="feather-trash-2 me-1"></i> Delete
                                </button>
                            </div>
                        `;
                    }
                }
            ],
            { searchPlaceholder: 'Search approval statuses... ', infoText: 'Showing _START_ to _END_ of _TOTAL_ approval statuses' }
        );

        $('#btnCreateApprovalStatus').on('click', function () {
            $('#approvalStatusId').val('');
            $('#approvalCode').val('');
            $('#approvalName').val('');
            $('#approvalModalLabel').text('Create Approval Status');
            showModal('approvalModal');
        });

        $('#approvalStatusesTable').on('click', '.btn-approval-edit', function () {
            const id = $(this).data('id');
            fetch(`${apis.approval}/${id}`)
                .then(r => (r.ok ? r.json() : Promise.reject(r)))
                .then(data => {
                    $('#approvalModalLabel').text('Edit Approval Status');
                    $('#approvalStatusId').val(data.approvalStatusId);
                    $('#approvalCode').val(data.code);
                    $('#approvalName').val(data.name);
                    showModal('approvalModal');
                })
                .catch(() => swalError('Error', { message: 'Unable to load approval status.' }));
        });

        $('#approvalStatusesTable').on('click', '.btn-approval-delete', function () {
            const id = $(this).data('id');
            confirmDelete('approval status').then(result => {
                if (!result.isConfirmed) return;
                fetch(`${apis.approval}/${id}`, { method: 'DELETE' })
                    .then(r => {
                        if (r.status === 204 || r.ok) return;
                        return r.json().then(b => Promise.reject(b));
                    })
                    .then(() => {
                        approvalTable.ajax.reload(null, false);
                        Swal.fire('Deleted', 'Approval status deleted.', 'success');
                    })
                    .catch(err => swalError('Error', err));
            });
        });

        $('#approvalForm').on('submit', function (e) {
            e.preventDefault();
            const id = $('#approvalStatusId').val();
            const payload = {
                approvalStatusId: id ? Number(id) : undefined,
                code: $('#approvalCode').val(),
                name: $('#approvalName').val()
            };
            const method = id ? 'PUT' : 'POST';
            const url = id ? `${apis.approval}/${id}` : apis.approval;
            fetch(url, {
                method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })
                .then(r => {
                    if (r.status === 201 || r.status === 204 || r.ok) return r.json().catch(() => ({}));
                    return r.json().then(b => Promise.reject(b));
                })
                .then(() => {
                    hideModal('approvalModal');
                    approvalTable.ajax.reload(null, false);
                    Swal.fire('Saved', 'Approval status saved.', 'success');
                })
                .catch(err => swalError('Error', err));
        });

        const cols = [
            { header: 'Code', value: r => r.code },
            { header: 'Name', value: r => r.name }
        ];

        $('#btnExportApprovalCsv').on('click', function (e) {
            e.preventDefault();
            exportToCsv(approvalTable, cols, 'approval_status');
        });
        $('#btnExportApprovalExcel').on('click', function (e) {
            e.preventDefault();
            exportToExcelHtml(approvalTable, cols, 'approval_status');
        });
    }

    function wireAttachment() {
        attachmentTable = initSimpleTable(
            '#attachmentTypesTable',
            apis.attachment,
            [
                { data: 'code' },
                { data: 'name' },
                {
                    data: 'isActive',
                    render: function (data) {
                        return data ? '<span class="badge bg-success">Active</span>' : '<span class="badge bg-secondary">Inactive</span>';
                    }
                },
                { data: 'appliesTo' },
                {
                    data: null,
                    orderable: false,
                    searchable: false,
                    render: function (data, type, row) {
                        return `
                            <div class="hstack gap-1 justify-content-center flex-nowrap">
                                <button type="button" class="btn btn-sm btn-light-brand btn-attachment-edit" data-id="${row.attachmentTypeId}">
                                    <i class="feather-edit-2 me-1"></i> Edit
                                </button>
                                <button type="button" class="btn btn-sm btn-light-danger btn-attachment-delete" data-id="${row.attachmentTypeId}">
                                    <i class="feather-trash-2 me-1"></i> Delete
                                </button>
                            </div>
                        `;
                    }
                }
            ],
            { searchPlaceholder: 'Search attachment types... ', infoText: 'Showing _START_ to _END_ of _TOTAL_ attachment types' }
        );

        $('#btnCreateAttachmentType').on('click', function () {
            $('#attachmentTypeId').val('');
            $('#attachmentCode').val('');
            $('#attachmentName').val('');
            $('#attachmentAppliesTo').val('');
            $('#attachmentIsActive').prop('checked', true);
            $('#attachmentModalLabel').text('Create Attachment Type');
            showModal('attachmentModal');
        });

        $('#attachmentTypesTable').on('click', '.btn-attachment-edit', function () {
            const id = $(this).data('id');
            fetch(`${apis.attachment}/${id}`)
                .then(r => (r.ok ? r.json() : Promise.reject(r)))
                .then(data => {
                    $('#attachmentModalLabel').text('Edit Attachment Type');
                    $('#attachmentTypeId').val(data.attachmentTypeId);
                    $('#attachmentCode').val(data.code);
                    $('#attachmentName').val(data.name);
                    $('#attachmentAppliesTo').val(data.appliesTo);
                    $('#attachmentIsActive').prop('checked', !!data.isActive);
                    showModal('attachmentModal');
                })
                .catch(() => swalError('Error', { message: 'Unable to load attachment type.' }));
        });

        $('#attachmentTypesTable').on('click', '.btn-attachment-delete', function () {
            const id = $(this).data('id');
            confirmDelete('attachment type').then(result => {
                if (!result.isConfirmed) return;
                fetch(`${apis.attachment}/${id}`, { method: 'DELETE' })
                    .then(r => {
                        if (r.status === 204 || r.ok) return;
                        return r.json().then(b => Promise.reject(b));
                    })
                    .then(() => {
                        attachmentTable.ajax.reload(null, false);
                        Swal.fire('Deleted', 'Attachment type deleted.', 'success');
                    })
                    .catch(err => swalError('Error', err));
            });
        });

        $('#attachmentForm').on('submit', function (e) {
            e.preventDefault();
            const id = $('#attachmentTypeId').val();
            const payload = {
                attachmentTypeId: id ? Number(id) : undefined,
                code: $('#attachmentCode').val(),
                name: $('#attachmentName').val(),
                appliesTo: $('#attachmentAppliesTo').val(),
                isActive: !!$('#attachmentIsActive').is(':checked')
            };
            const method = id ? 'PUT' : 'POST';
            const url = id ? `${apis.attachment}/${id}` : apis.attachment;
            fetch(url, {
                method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })
                .then(r => {
                    if (r.status === 201 || r.status === 204 || r.ok) return r.json().catch(() => ({}));
                    return r.json().then(b => Promise.reject(b));
                })
                .then(() => {
                    hideModal('attachmentModal');
                    attachmentTable.ajax.reload(null, false);
                    Swal.fire('Saved', 'Attachment type saved.', 'success');
                })
                .catch(err => swalError('Error', err));
        });

        const cols = [
            { header: 'Code', value: r => r.code },
            { header: 'Name', value: r => r.name },
            { header: 'Active', value: r => (r.isActive ? 'Active' : 'Inactive') },
            { header: 'Applies To', value: r => r.appliesTo }
        ];

        $('#btnExportAttachmentCsv').on('click', function (e) {
            e.preventDefault();
            exportToCsv(attachmentTable, cols, 'attachment_types');
        });
        $('#btnExportAttachmentExcel').on('click', function (e) {
            e.preventDefault();
            exportToExcelHtml(attachmentTable, cols, 'attachment_types');
        });
    }

    function wireContract() {
        contractTable = initSimpleTable(
            '#contractStatusesTable',
            apis.contract,
            [
                { data: 'code' },
                { data: 'name' },
                {
                    data: null,
                    orderable: false,
                    searchable: false,
                    render: function (data, type, row) {
                        return `
                            <div class="hstack gap-1 justify-content-center flex-nowrap">
                                <button type="button" class="btn btn-sm btn-light-brand btn-contract-edit" data-id="${row.contractStatusId}">
                                    <i class="feather-edit-2 me-1"></i> Edit
                                </button>
                                <button type="button" class="btn btn-sm btn-light-danger btn-contract-delete" data-id="${row.contractStatusId}">
                                    <i class="feather-trash-2 me-1"></i> Delete
                                </button>
                            </div>
                        `;
                    }
                }
            ],
            { searchPlaceholder: 'Search contract statuses... ', infoText: 'Showing _START_ to _END_ of _TOTAL_ contract statuses' }
        );

        $('#btnCreateContractStatus').on('click', function () {
            $('#contractStatusId').val('');
            $('#contractCode').val('');
            $('#contractName').val('');
            $('#contractModalLabel').text('Create Contract Status');
            showModal('contractModal');
        });

        $('#contractStatusesTable').on('click', '.btn-contract-edit', function () {
            const id = $(this).data('id');
            fetch(`${apis.contract}/${id}`)
                .then(r => (r.ok ? r.json() : Promise.reject(r)))
                .then(data => {
                    $('#contractModalLabel').text('Edit Contract Status');
                    $('#contractStatusId').val(data.contractStatusId);
                    $('#contractCode').val(data.code);
                    $('#contractName').val(data.name);
                    showModal('contractModal');
                })
                .catch(() => swalError('Error', { message: 'Unable to load contract status.' }));
        });

        $('#contractStatusesTable').on('click', '.btn-contract-delete', function () {
            const id = $(this).data('id');
            confirmDelete('contract status').then(result => {
                if (!result.isConfirmed) return;
                fetch(`${apis.contract}/${id}`, { method: 'DELETE' })
                    .then(r => {
                        if (r.status === 204 || r.ok) return;
                        return r.json().then(b => Promise.reject(b));
                    })
                    .then(() => {
                        contractTable.ajax.reload(null, false);
                        Swal.fire('Deleted', 'Contract status deleted.', 'success');
                    })
                    .catch(err => swalError('Error', err));
            });
        });

        $('#contractForm').on('submit', function (e) {
            e.preventDefault();
            const id = $('#contractStatusId').val();
            const payload = {
                contractStatusId: id ? Number(id) : undefined,
                code: $('#contractCode').val(),
                name: $('#contractName').val()
            };
            const method = id ? 'PUT' : 'POST';
            const url = id ? `${apis.contract}/${id}` : apis.contract;
            fetch(url, {
                method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })
                .then(r => {
                    if (r.status === 201 || r.status === 204 || r.ok) return r.json().catch(() => ({}));
                    return r.json().then(b => Promise.reject(b));
                })
                .then(() => {
                    hideModal('contractModal');
                    contractTable.ajax.reload(null, false);
                    Swal.fire('Saved', 'Contract status saved.', 'success');
                })
                .catch(err => swalError('Error', err));
        });

        const cols = [
            { header: 'Code', value: r => r.code },
            { header: 'Name', value: r => r.name }
        ];

        $('#btnExportContractCsv').on('click', function (e) {
            e.preventDefault();
            exportToCsv(contractTable, cols, 'contract_status');
        });
        $('#btnExportContractExcel').on('click', function (e) {
            e.preventDefault();
            exportToExcelHtml(contractTable, cols, 'contract_status');
        });
    }

    function wireInvoiceProgress() {
        invoiceProgressTable = initSimpleTable(
            '#invoiceProgressStatusesTable',
            apis.invoiceProgress,
            [
                { data: 'code' },
                { data: 'name' },
                {
                    data: null,
                    orderable: false,
                    searchable: false,
                    render: function (data, type, row) {
                        return `
                            <div class="hstack gap-1 justify-content-center flex-nowrap">
                                <button type="button" class="btn btn-sm btn-light-brand btn-invoiceprogress-edit" data-id="${row.progressStatusId}">
                                    <i class="feather-edit-2 me-1"></i> Edit
                                </button>
                                <button type="button" class="btn btn-sm btn-light-danger btn-invoiceprogress-delete" data-id="${row.progressStatusId}">
                                    <i class="feather-trash-2 me-1"></i> Delete
                                </button>
                            </div>
                        `;
                    }
                }
            ],
            { searchPlaceholder: 'Search invoice progress statuses... ', infoText: 'Showing _START_ to _END_ of _TOTAL_ invoice progress statuses' }
        );

        $('#btnCreateInvoiceProgressStatus').on('click', function () {
            $('#invoiceProgressStatusId').val('');
            $('#invoiceProgressCode').val('');
            $('#invoiceProgressName').val('');
            $('#invoiceProgressModalLabel').text('Create Invoice Progress Status');
            showModal('invoiceProgressModal');
        });

        $('#invoiceProgressStatusesTable').on('click', '.btn-invoiceprogress-edit', function () {
            const id = $(this).data('id');
            fetch(`${apis.invoiceProgress}/${id}`)
                .then(r => (r.ok ? r.json() : Promise.reject(r)))
                .then(data => {
                    $('#invoiceProgressModalLabel').text('Edit Invoice Progress Status');
                    $('#invoiceProgressStatusId').val(data.progressStatusId);
                    $('#invoiceProgressCode').val(data.code);
                    $('#invoiceProgressName').val(data.name);
                    showModal('invoiceProgressModal');
                })
                .catch(() => swalError('Error', { message: 'Unable to load invoice progress status.' }));
        });

        $('#invoiceProgressStatusesTable').on('click', '.btn-invoiceprogress-delete', function () {
            const id = $(this).data('id');
            confirmDelete('invoice progress status').then(result => {
                if (!result.isConfirmed) return;
                fetch(`${apis.invoiceProgress}/${id}`, { method: 'DELETE' })
                    .then(r => {
                        if (r.status === 204 || r.ok) return;
                        return r.json().then(b => Promise.reject(b));
                    })
                    .then(() => {
                        invoiceProgressTable.ajax.reload(null, false);
                        Swal.fire('Deleted', 'Invoice progress status deleted.', 'success');
                    })
                    .catch(err => swalError('Error', err));
            });
        });

        $('#invoiceProgressForm').on('submit', function (e) {
            e.preventDefault();
            const id = $('#invoiceProgressStatusId').val();
            const payload = {
                progressStatusId: id ? Number(id) : undefined,
                code: $('#invoiceProgressCode').val(),
                name: $('#invoiceProgressName').val()
            };
            const method = id ? 'PUT' : 'POST';
            const url = id ? `${apis.invoiceProgress}/${id}` : apis.invoiceProgress;
            fetch(url, {
                method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })
                .then(r => {
                    if (r.status === 201 || r.status === 204 || r.ok) return r.json().catch(() => ({}));
                    return r.json().then(b => Promise.reject(b));
                })
                .then(() => {
                    hideModal('invoiceProgressModal');
                    invoiceProgressTable.ajax.reload(null, false);
                    Swal.fire('Saved', 'Invoice progress status saved.', 'success');
                })
                .catch(err => swalError('Error', err));
        });

        const cols = [
            { header: 'Code', value: r => r.code },
            { header: 'Name', value: r => r.name }
        ];

        $('#btnExportInvoiceProgressCsv').on('click', function (e) {
            e.preventDefault();
            exportToCsv(invoiceProgressTable, cols, 'invoice_progress_status');
        });
        $('#btnExportInvoiceProgressExcel').on('click', function (e) {
            e.preventDefault();
            exportToExcelHtml(invoiceProgressTable, cols, 'invoice_progress_status');
        });
    }

    function wireVendorCategory() {
        vendorCategoryTable = initSimpleTable(
            '#vendorCategoriesTable',
            apis.vendorCategory,
            [
                { data: 'name' },
                {
                    data: null,
                    orderable: false,
                    searchable: false,
                    render: function (data, type, row) {
                        return `
                            <div class="hstack gap-1 justify-content-center flex-nowrap">
                                <button type="button" class="btn btn-sm btn-light-brand btn-vendorcat-edit" data-id="${row.vendorCategoryId}">
                                    <i class="feather-edit-2 me-1"></i> Edit
                                </button>
                                <button type="button" class="btn btn-sm btn-light-danger btn-vendorcat-delete" data-id="${row.vendorCategoryId}">
                                    <i class="feather-trash-2 me-1"></i> Delete
                                </button>
                            </div>
                        `;
                    }
                }
            ],
            { searchPlaceholder: 'Search vendor categories... ', infoText: 'Showing _START_ to _END_ of _TOTAL_ vendor categories' }
        );

        $('#btnCreateVendorCategory').on('click', function () {
            $('#vendorCategoryId').val('');
            $('#vendorCategoryName').val('');
            $('#vendorCategoryModalLabel').text('Create Vendor Category');
            showModal('vendorCategoryModal');
        });

        $('#vendorCategoriesTable').on('click', '.btn-vendorcat-edit', function () {
            const id = $(this).data('id');
            fetch(`${apis.vendorCategory}/${id}`)
                .then(r => (r.ok ? r.json() : Promise.reject(r)))
                .then(data => {
                    $('#vendorCategoryModalLabel').text('Edit Vendor Category');
                    $('#vendorCategoryId').val(data.vendorCategoryId);
                    $('#vendorCategoryName').val(data.name);
                    showModal('vendorCategoryModal');
                })
                .catch(() => swalError('Error', { message: 'Unable to load vendor category.' }));
        });

        $('#vendorCategoriesTable').on('click', '.btn-vendorcat-delete', function () {
            const id = $(this).data('id');
            confirmDelete('vendor category').then(result => {
                if (!result.isConfirmed) return;
                fetch(`${apis.vendorCategory}/${id}`, { method: 'DELETE' })
                    .then(r => {
                        if (r.status === 204 || r.ok) return;
                        return r.json().then(b => Promise.reject(b));
                    })
                    .then(() => {
                        vendorCategoryTable.ajax.reload(null, false);
                        Swal.fire('Deleted', 'Vendor category deleted.', 'success');
                    })
                    .catch(err => swalError('Error', err));
            });
        });

        $('#vendorCategoryForm').on('submit', function (e) {
            e.preventDefault();
            const id = $('#vendorCategoryId').val();
            const payload = {
                vendorCategoryId: id ? Number(id) : undefined,
                name: $('#vendorCategoryName').val()
            };
            const method = id ? 'PUT' : 'POST';
            const url = id ? `${apis.vendorCategory}/${id}` : apis.vendorCategory;
            fetch(url, {
                method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })
                .then(r => {
                    if (r.status === 201 || r.status === 204 || r.ok) return r.json().catch(() => ({}));
                    return r.json().then(b => Promise.reject(b));
                })
                .then(() => {
                    hideModal('vendorCategoryModal');
                    vendorCategoryTable.ajax.reload(null, false);
                    Swal.fire('Saved', 'Vendor category saved.', 'success');
                })
                .catch(err => swalError('Error', err));
        });

        const cols = [{ header: 'Name', value: r => r.name }];

        $('#btnExportVendorCategoryCsv').on('click', function (e) {
            e.preventDefault();
            exportToCsv(vendorCategoryTable, cols, 'vendor_categories');
        });
        $('#btnExportVendorCategoryExcel').on('click', function (e) {
            e.preventDefault();
            exportToExcelHtml(vendorCategoryTable, cols, 'vendor_categories');
        });
    }

    function wireTabAdjustments() {
        document.querySelectorAll('button[data-bs-toggle="tab"]').forEach(btn => {
            btn.addEventListener('shown.bs.tab', function (event) {
                const target = event.target.getAttribute('data-bs-target');
                // Adjust columns for tables inside newly shown tabs
                if (target === '#tab-approval' && approvalTable) approvalTable.columns.adjust();
                if (target === '#tab-attachment' && attachmentTable) attachmentTable.columns.adjust();
                if (target === '#tab-contract' && contractTable) contractTable.columns.adjust();
                if (target === '#tab-invoice-progress' && invoiceProgressTable) invoiceProgressTable.columns.adjust();
                if (target === '#tab-vendor-category' && vendorCategoryTable) vendorCategoryTable.columns.adjust();
            });
        });
    }

    $(function () {
        // Fix template blur by ensuring modals are direct children of <body>
        portalModalToBody('approvalModal');
        portalModalToBody('attachmentModal');
        portalModalToBody('contractModal');
        portalModalToBody('invoiceProgressModal');
        portalModalToBody('vendorCategoryModal');

        wireApproval();
        wireAttachment();
        wireContract();
        wireInvoiceProgress();
        wireVendorCategory();
        wireTabAdjustments();
    });
})(jQuery);
