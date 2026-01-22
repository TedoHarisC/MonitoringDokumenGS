/* master-vendor.js
   - DataTable listing vendors
   - Create/Edit modal using Bootstrap 5
   - Uses Vendors API at /api/v1/Vendors
*/

;(function ($) {
    'use strict';

    let table;
    const apiBase = '/api/vendors';
    const vendorCategoriesApiBase = '/api/vendor-categories';

    let vendorCategoriesCache = null;

    function normalizeToArray(json) {
        if (!json) return [];
        if (Array.isArray(json)) return json;
        if (Array.isArray(json.items)) return json.items;
        if (Array.isArray(json.data)) return json.data;
        return [];
    }

    function populateVendorCategorySelect(items) {
        const select = document.getElementById('vendorCategoryId');
        if (!select) return;

        const currentValue = select.value;
        select.innerHTML = '<option value="">-- Select Category --</option>';

        for (const cat of items) {
            const option = document.createElement('option');
            option.value = String(cat.vendorCategoryId ?? cat.VendorCategoryId ?? '');
            option.textContent = String(cat.name ?? cat.Name ?? option.value);
            select.appendChild(option);
        }

        // Preserve current selection if possible
        if (currentValue) select.value = currentValue;
    }

    function getCategoryNameById(id) {
        const numericId = Number(id);
        if (!numericId || !vendorCategoriesCache) return null;
        const match = vendorCategoriesCache.find(x => Number(x.vendorCategoryId ?? x.VendorCategoryId) === numericId);
        if (!match) return null;
        return String(match.name ?? match.Name ?? '').trim() || null;
    }

    async function ensureVendorCategoriesLoaded() {
        if (vendorCategoriesCache) return vendorCategoriesCache;
        try {
            const url = `${vendorCategoriesApiBase}?page=1&pageSize=1000`;
            const res = await fetch(url);
            if (!res.ok) throw res;
            const json = await res.json();
            vendorCategoriesCache = normalizeToArray(json);
            populateVendorCategorySelect(vendorCategoriesCache);
            return vendorCategoriesCache;
        } catch (e) {
            vendorCategoriesCache = [];
            populateVendorCategorySelect([]);
            return vendorCategoriesCache;
        }
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

    async function exportVendors(kind) {
        await ensureVendorCategoriesLoaded();
        const columns = [
            { header: 'Vendor Code', value: r => r.vendorCode ?? '' },
            { header: 'Vendor Name', value: r => r.vendorName ?? '' },
            { header: 'Short Name', value: r => r.shortName ?? '' },
            {
                header: 'Category',
                value: r => {
                    const name = getCategoryNameById(r.vendorCategoryId);
                    return name ?? r.vendorCategoryId ?? '';
                }
            },
            { header: 'Owner', value: r => r.ownerName ?? '' },
            { header: 'Company Email', value: r => r.companyEmail ?? '' },
            { header: 'NPWP', value: r => r.npwp ?? '' }
        ];

        if (kind === 'csv') return exportToCsv(table, columns, 'Vendors');
        if (kind === 'excel') return exportToExcelHtml(table, columns, 'Vendors');
    }

    function initTable() {
        table = $('#vendorsTable').DataTable({
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
                url: apiBase,
                dataSrc: function (json) {
                    // If API returns paged response or plain array
                    if (!json) return [];
                    if (Array.isArray(json)) return json;
                    if (json.items) return json.items;
                    return json;
                }
            },
            columns: [
                { data: 'vendorCode' },
                { data: 'vendorName' },
                { data: 'shortName' },
                {
                    data: 'vendorCategoryId',
                    render: function (data) {
                        const name = getCategoryNameById(data);
                        return name ?? data ?? '';
                    }
                },
                { data: 'ownerName' },
                { data: 'companyEmail' },
                { data: 'npwp' },
                {
                    data: null,
                    orderable: false,
                    searchable: false,
                    render: function (data, type, row) {
                        return `
                            <div class="hstack gap-1 justify-content-center flex-nowrap">
                                <button type="button" class="btn btn-sm btn-light-brand btn-edit" data-id="${row.vendorId}">
                                    <i class="feather-edit-2 me-1"></i> Edit
                                </button>
                                <button type="button" class="btn btn-sm btn-light-danger btn-delete" data-id="${row.vendorId}">
                                    <i class="feather-trash-2 me-1"></i> Delete
                                </button>
                            </div>
                        `;
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
                searchPlaceholder: 'Search vendors... ',
                lengthMenu: '_MENU_ / page',
                info: 'Showing _START_ to _END_ of _TOTAL_ vendors',
                infoEmpty: 'No vendors found',
                zeroRecords: 'No matching vendors',
                paginate: {
                    previous: '<i class="feather-chevron-left"></i>',
                    next: '<i class="feather-chevron-right"></i>'
                }
            },
            columnDefs: [
                { targets: [0, 2, 6], className: 'text-nowrap' },
                {
                    targets: [7],
                    className: 'text-center text-nowrap',
                    width: 170,
                    createdCell: function (td) {
                        td.classList.add('dt-actions');
                    }
                }
            ]
        });
    }

    function openCreate() {
        clearForm();
        $('#vendorModalLabel').text('Create Vendor');
        ensureVendorCategoriesLoaded().finally(() => {
            $('#vendorCategoryId').val('');
            const modal = new bootstrap.Modal(document.getElementById('vendorModal'));
            modal.show();
        });
    }

    function openEdit(id) {
        fetch(`${apiBase}/${id}`)
            .then(r => {
                if (!r.ok) throw r;
                return r.json();
            })
            .then(data => {
                $('#vendorModalLabel').text('Edit Vendor');
                $('#vendorId').val(data.vendorId);
                $('#vendorCode').val(data.vendorCode);
                $('#vendorName').val(data.vendorName);
                $('#shortName').val(data.shortName);
                return ensureVendorCategoriesLoaded().then(() => {
                    $('#vendorCategoryId').val(String(data.vendorCategoryId ?? ''));
                    return data;
                });
            })
            .then(data => {
                $('#npwp').val(data.npwp);
                $('#ownerName').val(data.ownerName);
                $('#ownerPhone').val(data.ownerPhone);
                $('#companyEmail').val(data.companyEmail);
                const modal = new bootstrap.Modal(document.getElementById('vendorModal'));
                modal.show();
            })
            .catch(err => {
                Swal.fire('Error', 'Unable to load vendor', 'error');
            });
    }

    function clearForm() {
        $('#vendorId').val('');
        $('#vendorCode').val('');
        $('#vendorName').val('');
        $('#shortName').val('');
        $('#vendorCategoryId').val('');
        $('#npwp').val('');
        $('#ownerName').val('');
        $('#ownerPhone').val('');
        $('#companyEmail').val('');
    }

    function saveVendor(e) {
        e.preventDefault();
        const id = $('#vendorId').val();
        const payload = {
            vendorId: id || undefined,
            vendorCode: $('#vendorCode').val(),
            vendorName: $('#vendorName').val(),
            shortName: $('#shortName').val(),
            vendorCategoryId: Number($('#vendorCategoryId').val() || 0),
            ownerName: $('#ownerName').val(),
            ownerPhone: $('#ownerPhone').val(),
            companyEmail: $('#companyEmail').val(),
            npwp: $('#npwp').val(),
        };

        const method = id ? 'PUT' : 'POST';
        const url = id ? `${apiBase}/${id}` : apiBase;

        fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        })
            .then(r => {
                if (r.status === 201 || r.status === 204 || r.ok) return r.json().catch(() => ({}));
                return r.json().then(b => { throw b; });
            })
            .then(() => {
                const modalEl = document.getElementById('vendorModal');
                const modal = bootstrap.Modal.getInstance(modalEl);
                modal.hide();
                table.ajax.reload(null, false);
                Swal.fire('Saved', 'Vendor saved successfully', 'success');
            })
            .catch(err => {
                const message = err && (err.message || err.errors && err.errors.join(', ')) || 'Unable to save';
                Swal.fire('Error', message, 'error');
            });
    }

    function deleteVendor(id) {
        Swal.fire({
            title: 'Delete vendor? ',
            text: 'This action cannot be undone.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Delete',
            cancelButtonText: 'Cancel'
        }).then(result => {
            if (!result.isConfirmed) return;
            fetch(`${apiBase}/${id}`, { method: 'DELETE' })
                .then(r => {
                    if (r.status === 204 || r.ok) {
                        table.ajax.reload(null, false);
                        Swal.fire('Deleted', 'Vendor deleted', 'success');
                    } else {
                        return r.json().then(b => { throw b; });
                    }
                })
                .catch(err => {
                    const message = err && (err.message || err.errors && err.errors.join(', ')) || 'Unable to delete';
                    Swal.fire('Error', message, 'error');
                });
        });
    }

    $(async function () {
        // Template applies blur on body.modal-open to .nxl-container; if the modal lives inside it,
        // the modal gets blurred too. Move modal to <body> to avoid that.
        const vendorModalEl = document.getElementById('vendorModal');
        if (vendorModalEl && vendorModalEl.parentElement !== document.body) {
            document.body.appendChild(vendorModalEl);
        }

        // Load categories first so the table can immediately display Category Name.
        await ensureVendorCategoriesLoaded();
        initTable();

        $('#btnCreate').on('click', openCreate);

        $('#vendorsTable').on('click', '.btn-edit', function () {
            const id = $(this).data('id');
            openEdit(id);
        });

        $('#vendorsTable').on('click', '.btn-delete', function () {
            const id = $(this).data('id');
            deleteVendor(id);
        });

        const btnExportCsv = document.getElementById('btnExportVendorsCsv');
        if (btnExportCsv) {
            btnExportCsv.addEventListener('click', async function (e) {
                e.preventDefault();
                await exportVendors('csv');
            });
        }

        const btnExportExcel = document.getElementById('btnExportVendorsExcel');
        if (btnExportExcel) {
            btnExportExcel.addEventListener('click', async function (e) {
                e.preventDefault();
                await exportVendors('excel');
            });
        }

        $('#vendorForm').on('submit', saveVendor);
    });

})(jQuery);
