// ==============================
// Budget Management JavaScript
// ==============================

let budgetTable;
let budgetModal;
let isEditMode = false;

$(document).ready(function () {
    // Initialize modal
    budgetModal = new bootstrap.Modal(document.getElementById('budgetModal'));

    // Initialize DataTable
    initDataTable();

    // Event: Add Budget Button
    $('#btnAddBudget').on('click', function () {
        openAddModal();
    });

    // Event: Form Submit
    $('#budgetForm').on('submit', function (e) {
        e.preventDefault();
        saveBudget();
    });

    // Format currency inputs
    $('#totalBudget, #monthlyBudget').on('input', function () {
        formatCurrencyInput(this);
    });

    // Auto calculate monthly budget
    $('#totalBudget').on('blur', function () {
        autoCalculateMonthly();
    });
});

// Initialize DataTable
function initDataTable() {
    budgetTable = $('#budgetTable').DataTable({
        processing: true,
        serverSide: false,
        ajax: {
            url: '/api/budgets',
            type: 'GET',
            dataSrc: function (json) {
                return json.items || json;
            },
            error: function (xhr, error, thrown) {
                console.error('DataTable error:', error, thrown);
                showAlert('Error loading data: ' + (xhr.responseJSON?.message || 'Unknown error'), 'danger');
            }
        },
        columns: [
            { data: 'year', className: 'fw-bold' },
            {
                data: 'totalBudget',
                render: function (data) {
                    return formatRupiah(data);
                }
            },
            {
                data: 'monthlyBudget',
                render: function (data) {
                    return formatRupiah(data);
                }
            },
            {
                data: 'createdAt',
                render: function (data) {
                    return data ? new Date(data).toLocaleDateString('id-ID') : '-';
                }
            },
            {
                data: null,
                className: 'text-end dt-actions',
                orderable: false,
                render: function (data, type, row) {
                    return `
                        <div class="hstack gap-2 justify-content-end">
                            <a href="javascript:void(0);" class="avatar-text avatar-md" onclick="editBudget('${row.budgetId}')">
                                <i class="feather feather-edit-3"></i>
                            </a>
                            <a href="javascript:void(0);" class="avatar-text avatar-md" onclick="deleteBudget('${row.budgetId}', ${row.year})">
                                <i class="feather feather-trash-2"></i>
                            </a>
                        </div>
                    `;
                }
            }
        ],
        order: [[0, 'desc']],
        pageLength: 10,
        language: {
            emptyTable: "No budget data available",
            zeroRecords: "No matching budgets found"
        }
    });
}

// Open Add Modal
function openAddModal() {
    isEditMode = false;
    $('#budgetModalLabel').text('Add Budget');
    $('#budgetForm')[0].reset();
    $('#budgetId').val('');
    budgetModal.show();
}

// Edit Budget
function editBudget(budgetId) {
    isEditMode = true;
    $('#budgetModalLabel').text('Edit Budget');

    $.ajax({
        url: `/api/budgets/${budgetId}`,
        type: 'GET',
        success: function (data) {
            $('#budgetId').val(data.budgetId);
            $('#year').val(data.year);
            $('#totalBudget').val(formatNumberForInput(data.totalBudget));
            $('#monthlyBudget').val(formatNumberForInput(data.monthlyBudget));
            budgetModal.show();
        },
        error: function (xhr) {
            showAlert('Failed to load budget data: ' + (xhr.responseJSON?.message || 'Unknown error'), 'danger');
        }
    });
}

// Save Budget (Create or Update)
function saveBudget() {
    const budgetId = $('#budgetId').val();
    const data = {
        budgetId: budgetId || '00000000-0000-0000-0000-000000000000',
        year: parseInt($('#year').val()),
        totalBudget: parseFloat($('#totalBudget').val().replace(/\./g, '').replace(',', '.')),
        monthlyBudget: parseFloat($('#monthlyBudget').val().replace(/\./g, '').replace(',', '.')),
        createdBy: '00000000-0000-0000-0000-000000000000' // Will be set by server
    };

    const url = isEditMode ? `/api/budgets/${budgetId}` : '/api/budgets';
    const method = isEditMode ? 'PUT' : 'POST';

    $.ajax({
        url: url,
        type: method,
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            budgetModal.hide();
            budgetTable.ajax.reload();
            showAlert(isEditMode ? 'Budget updated successfully!' : 'Budget created successfully!', 'success');
        },
        error: function (xhr) {
            const errorMsg = xhr.responseJSON?.message || xhr.responseJSON?.title || 'Failed to save budget';
            showAlert(errorMsg, 'danger');
        }
    });
}

// Delete Budget
function deleteBudget(budgetId, year) {
    if (confirm(`Are you sure you want to delete budget for year ${year}?`)) {
        $.ajax({
            url: `/api/budgets/${budgetId}`,
            type: 'DELETE',
            success: function () {
                budgetTable.ajax.reload();
                showAlert('Budget deleted successfully!', 'success');
            },
            error: function (xhr) {
                showAlert('Failed to delete budget: ' + (xhr.responseJSON?.message || 'Unknown error'), 'danger');
            }
        });
    }
}

// Auto Calculate Monthly Budget
function autoCalculateMonthly() {
    const totalBudget = parseFloat($('#totalBudget').val().replace(/\./g, '').replace(',', '.'));
    if (!isNaN(totalBudget) && totalBudget > 0) {
        const monthlyBudget = totalBudget / 12;
        $('#monthlyBudget').val(formatNumberForInput(monthlyBudget));
    }
}

// Format Currency Input (Rupiah style)
function formatCurrencyInput(input) {
    let value = input.value.replace(/\D/g, '');
    if (value) {
        value = parseInt(value).toLocaleString('id-ID');
    }
    input.value = value;
}

// Format Number for Input
function formatNumberForInput(number) {
    if (!number) return '';
    return Math.round(number).toLocaleString('id-ID');
}

// Format Rupiah for Display
function formatRupiah(amount) {
    if (!amount && amount !== 0) return 'Rp 0';
    return 'Rp ' + Math.round(amount).toLocaleString('id-ID');
}

// Show Alert
function showAlert(message, type = 'info') {
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    
    // Remove existing alerts
    $('.main-content .alert').remove();
    
    // Add new alert at the top of main content
    $('.main-content').prepend(alertHtml);
    
    // Auto dismiss after 5 seconds
    setTimeout(function () {
        $('.main-content .alert').fadeOut('slow', function () {
            $(this).remove();
        });
    }, 5000);
}
