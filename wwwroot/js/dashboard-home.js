// ==============================
// Invoice Total by Status (for #invoice_by_status_area)
// ==============================
function loadInvoiceByStatusArea() {
    $.ajax({
        url: '/api/dashboard/invoice-status-summary',
        method: 'GET',
        success: function (data) {
            const area = $('#invoice_by_status_area');
            area.empty();
            // Ambil semua status dari API (selalu muncul semua status meski 0)
            let statusList = [];
            if (data && data.statusCounts) {
                statusList = data.statusCounts;
            }
            if (!statusList || statusList.length === 0) {
                area.html('<div class="col-12 text-center text-muted py-4">No status data available</div>');
                return;
            }
            // Bungkus card dalam row dan center jika kurang dari 6
            let html = '<div class="row justify-content-center">';
            statusList.forEach(function (item) {
                let color = 'secondary';
                let icon = 'feather-info';
                switch (item.progressStatusName) {
                    case 'Verifikasi GS': color = 'info'; icon = 'feather-info'; break;
                    case 'Done': color = 'success'; icon = 'feather-check-circle'; break;
                    case 'Approved by HCGS Dept': color = 'warning'; icon = 'feather-thumbs-up'; break;
                    case 'Approved by KTT': color = 'primary'; icon = 'feather-thumbs-up'; break;
                    case 'Validasi & Pembayaran FA': color = 'secondary'; icon = 'feather-dollar-sign'; break;
                    case 'Approved': color = 'primary'; icon = 'feather-thumbs-up'; break;
                    case 'Rejected': color = 'danger'; icon = 'feather-x-circle'; break;
                }

                let progressStatusName = (item.progressStatusName == 'Validasi & Pembayaran FA') ? 'Pembayaran' : (item.progressStatusName == 'Approved by HCGS Dept') ? 'Approved by HCGS' : item.progressStatusName;
                html += `
                <div class="col-xl-2 col-lg-3 col-md-4 col-6 mb-3">
                    <div class="p-3 border border-dashed rounded invoice-status-card" 
                         data-status="${item.progressStatusName}">
                        <div class="fs-12 text-muted mb-1">${progressStatusName}</div>
                        <div class="d-flex align-items-center gap-2">
                            <h3 class="fw-bold text-${color} mb-0">${item.total}</h3>
                            <span class="badge bg-${color}-subtle text-${color}">
                                <i class="${icon}"></i>
                            </span>
                        </div>
                        <div class="fs-11 text-muted mt-1">Total</div>
                    </div>
                </div>
                `;
            });
            html += `
                <div class="col-xl-2 col-lg-3 col-md-4 col-6 mb-3">
                            <div class="p-3 border border-dashed rounded">
                                <div class="fs-12 text-muted mb-1">Rejected</div>
                                <div class="d-flex align-items-center gap-2">
                                    <h3 class="fw-bold text-danger mb-0" id="activeContractsCount">0</h3>
                                    <span class="badge bg-danger-subtle text-danger">
                                        <i class="feather-x-circle"></i>
                                    </span>
                                </div>
                                <div class="fs-11 text-muted mt-1">Total</div>
                            </div>
                        </div>
            `;
            html += '</div>';
            area.html(html);
        },
        error: function () {
            $('#invoice_by_status_area').html('<div class="col-12 text-center text-danger py-4">Failed to load data</div>');
        }
    });
}
// ==============================
// Dashboard Home Charts
// ==============================

// Visitors Overview Statistics Chart
function initVisitorsOverviewChart() {
    // Guard: jangan render jika elemen tidak ada di DOM
    if (!document.querySelector("#visitors-overview-statistics-chart")) return;

    const visitorsOverviewOptions = {
        series: [{
            name: 'Visitors',
            data: [31, 40, 28, 51, 42, 85, 77, 65, 90, 120, 95, 110]
        }],
        chart: {
            type: 'area',
            height: 350,
            toolbar: {
                show: false
            }
        },
        colors: ['#3b76ef'],
        dataLabels: {
            enabled: false
        },
        stroke: {
            curve: 'smooth',
            width: 2
        },
        xaxis: {
            categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']
        },
        yaxis: {
            title: {
                text: 'Visitors'
            }
        },
        fill: {
            type: 'gradient',
            gradient: {
                shadeIntensity: 1,
                opacityFrom: 0.7,
                opacityTo: 0.3,
                stops: [0, 90, 100]
            }
        },
        tooltip: {
            theme: 'light'
        }
    };

    const visitorsChart = new ApexCharts(document.querySelector("#visitors-overview-statistics-chart"), visitorsOverviewOptions);
    visitorsChart.render();
}

// Social Radar Chart
function initSocialRadarChart() {
    // Guard: jangan render jika elemen tidak ada di DOM
    if (!document.querySelector("#social-radar-chart")) return;

    const socialRadarOptions = {
        series: [{
            name: 'Engagement',
            data: [80, 50, 30, 40, 100, 20],
        }],
        chart: {
            height: 350,
            type: 'radar',
            toolbar: {
                show: false
            }
        },
        colors: ['#3b76ef'],
        xaxis: {
            categories: ['Facebook', 'Twitter', 'Instagram', 'LinkedIn', 'YouTube', 'TikTok']
        },
        yaxis: {
            show: false
        },
        fill: {
            opacity: 0.2
        },
        stroke: {
            show: true,
            width: 2,
            colors: ['#3b76ef'],
            dashArray: 0
        },
        markers: {
            size: 4,
            colors: ['#3b76ef'],
            strokeColors: '#fff',
            strokeWidth: 2,
        }
    };

    const socialRadarChart = new ApexCharts(document.querySelector("#social-radar-chart"), socialRadarOptions);
    socialRadarChart.render();
}

// Top 5 Vendor Spend Chart
let topVendorChart = null;

// Load Top Vendor Spend Chart
function loadTopVendorChart() {
    $.ajax({
        url: '/api/dashboard/top-vendors?top=5',
        type: 'GET',
        success: function (data) {
            if (data && data.length > 0) {
                const vendorNames = data.map(v => v.vendorName);
                const vendorSpends = data.map(v => v.totalSpend);

                const topVendorOptions = {
                    series: [{
                        name: 'Total Spend',
                        data: vendorSpends
                    }],
                    chart: {
                        type: 'bar',
                        height: 350,
                        toolbar: {
                            show: false
                        }
                    },
                    colors: ['#3b76ef'],
                    plotOptions: {
                        bar: {
                            horizontal: true,
                            borderRadius: 4,
                            dataLabels: {
                                position: 'top'
                            }
                        }
                    },
                    dataLabels: {
                        enabled: true,
                        formatter: function (val) {
                            return 'Rp ' + val.toLocaleString('id-ID');
                        },
                        offsetX: -6,
                        style: {
                            fontSize: '11px',
                            colors: ['#fff']
                        }
                    },
                    xaxis: {
                        categories: vendorNames,
                        labels: {
                            formatter: function (val) {
                                return 'Rp ' + (val / 1000000).toFixed(1) + 'M';
                            }
                        }
                    },
                    yaxis: {
                        title: {
                            text: 'Vendor'
                        }
                    },
                    tooltip: {
                        theme: 'light',
                        y: {
                            formatter: function (val) {
                                return 'Rp ' + val.toLocaleString('id-ID');
                            }
                        }
                    }
                };

                // Destroy existing chart if any
                if (topVendorChart) {
                    topVendorChart.destroy();
                }

                topVendorChart = new ApexCharts(document.querySelector("#top-vendor-spend-chart"), topVendorOptions);
                topVendorChart.render();
            } else {
                $('#top-vendor-spend-chart').html('<div class="text-center py-5 text-muted">No vendor data available</div>');
            }
        },
        error: function (xhr) {
            console.error('Failed to load top vendors:', xhr);
            $('#top-vendor-spend-chart').html('<div class="text-center py-5 text-danger">Failed to load data</div>');
        }
    });
}

// Initialize all charts on page load
$(document).ready(function () {
        // Bind click event for dynamically rendered status cards (delegated)
        $(document).on('click', '.invoice-status-card', function () {
            const status = $(this).data('status');
            showInvoicesByStatusModal(status);
        });

        // Saat modal terbuka: fokus ke modal-body bukan close button
        $('#invoiceStatusModal').on('shown.bs.modal', function () {
            $(this).find('.modal-body').attr('tabindex', '-1').trigger('focus');
        });

        // Saat modal AKAN ditutup: blur dulu sebelum aria-hidden ditambahkan
        $('#invoiceStatusModal').on('hide.bs.modal', function () {
            if (document.activeElement) {
                document.activeElement.blur();
            }
        });

        // Saat modal sudah tertutup: cleanup DataTable
        $('#invoiceStatusModal').on('hidden.bs.modal', function () {
            if ($.fn.DataTable.isDataTable('#modalInvoicesTable')) {
                $('#modalInvoicesTable').DataTable().clear().destroy();
            }
            $('#modalInvoicesTable tbody').empty();
        });

    });

    // Show modal and load invoices by status
    function showInvoicesByStatusModal(status) {
        // Set modal title
        $('#invoiceStatusModalLabel').text('Invoice List - ' + status);
        // Show modal
        var modal = new bootstrap.Modal(document.getElementById('invoiceStatusModal'));
        modal.show();

        // Fetch invoices by status
        $.ajax({
            url: '/api/invoices/filter-by-status?status=' + encodeURIComponent(status),
            method: 'GET',
            success: function (data) {
                let rows = '';
                let isEmpty = !data || data.length === 0;

                //console.log(data);

                if (!isEmpty) {
                    const monthNames = [
                        '', 'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
                        'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'
                    ];
                    data.forEach(function (inv) {
                        // Pastikan field yang dipakai sesuai API
                        const invoiceAmount = (inv.invoiceAmount !== undefined && inv.invoiceAmount !== null)
                            ? inv.invoiceAmount.toLocaleString('id-ID', { style: 'currency', currency: 'IDR' })
                            : '';
                        const taxAmount = (inv.taxAmount !== undefined && inv.taxAmount !== null)
                            ? inv.taxAmount.toLocaleString('id-ID', { style: 'currency', currency: 'IDR' })
                            : '';
                        const year = inv.invoiceYear || '';
                        const monthNum = inv.invoiceMonth || 0;
                        const month = monthNames[monthNum] || inv.invoiceMonth || '';
                        rows += `<tr>
                            <td>${inv.invoiceNumber || ''}</td>
                            <td>${inv.vendorName || ''}</td>
                            <td>${inv.progressStatusName || ''}</td>
                            <td class="text-end">${invoiceAmount}</td>
                            <td class="text-end">${taxAmount}</td>
                            <td class="text-center">${year}</td>
                            <td class="text-center">${month}</td>
                            <td class="text-center">${inv.isOnTime ? 'On Time' : 'Late'}</td>
                            <td>${inv.createdAt ? new Date(inv.createdAt).toLocaleString('id-ID') : ''}</td>
                            <td class="text-center">
                                <a href="/Invoice/Details/${inv.invoiceId || inv.id}" class="btn btn-sm btn-info">Detail</a>
                            </td>
                        </tr>`;
                    });
                }

                // Destroy DataTable if exists
                if ($.fn.DataTable.isDataTable('#modalInvoicesTable')) {
                    $('#modalInvoicesTable').DataTable().destroy();
                }

                // Kosongkan tbody, lalu isi jika ada data
                $('#modalInvoicesTable tbody').html(isEmpty ? '' : rows);

                // Pastikan DataTables Buttons sudah dimuat
                function ensureDataTablesButtons(callback) {
                    if ($.fn.dataTable && $.fn.dataTable.Buttons) {
                        callback();
                        return;
                    }
                    // Load loader script jika belum ada
                    if (!window._dtButtonsLoaderInjected) {
                        var s = document.createElement('script');
                        s.src = '/js/datatables-buttons-loader.js';
                        document.head.appendChild(s);
                        window._dtButtonsLoaderInjected = true;
                    }
                    var tryCount = 0;
                    function waitForButtons() {
                        if ($.fn.dataTable && $.fn.dataTable.Buttons) {
                            callback();
                        } else if (tryCount < 20) {
                            tryCount++;
                            setTimeout(waitForButtons, 150);
                        } else {
                            callback(); // fallback: tetap inisialisasi tanpa export
                        }
                    }
                    waitForButtons();
                }

                ensureDataTablesButtons(function() {
                    $('#modalInvoicesTable').DataTable({
                        responsive: true,
                        order: [],
                        autoWidth: false,
                        searching: false,
                        paging: false,
                        info: false,
                        language: {
                            emptyTable: "No invoices found for this status."
                        },
                        dom: 'Bfrtip',
                        buttons: [
                            {
                                extend: 'excelHtml5',
                                text: '<i class="feather-download"></i> Export Excel',
                                className: 'btn btn-success btn-sm',
                                title: 'Invoice List - ' + status,
                                exportOptions: {
                                    columns: ':visible:not(:last-child)'
                                }
                            }
                        ]
                    });
                });
            },
            error: function () {
                if ($.fn.DataTable.isDataTable('#modalInvoicesTable')) {
                    $('#modalInvoicesTable').DataTable().destroy();
                }
                $('#modalInvoicesTable tbody').html('');
                $('#modalInvoicesTable').DataTable({
                    responsive: true,
                    order: [],
                    autoWidth: false,
                    searching: false,
                    paging: false,
                    info: false,
                    language: {
                        emptyTable: "Failed to load invoices."
                    }
                });
            }
        });
}

// Document ready block
$(document).ready(function () {
    // Load Dashboard Statistics (Contracts & Invoices)
    loadDashboardStats();

    // Load Invoice By Status Area
    loadInvoiceByStatusArea();

    // Load Budget KPI Dashboard
    const currentYear = new Date().getFullYear();
    $('#budgetYearFilter').val(currentYear);
    loadBudgetSummary(currentYear);
    loadBudgetKpiChart(currentYear);
    loadMonthlyRealisasiChart(currentYear);

    // Year filter change event
    $('#budgetYearFilter').on('change', function () {
        const selectedYear = parseInt($(this).val());
        $('#budget-year-display').text(selectedYear);
        loadBudgetSummary(selectedYear);
        loadBudgetKpiChart(selectedYear);
        loadMonthlyRealisasiChart(selectedYear);
    });

    // Load Vendor On-Time Submission KPI
    const $ontimeFilter = $('#ontimeYearFilter');
    if ($ontimeFilter.length > 0) {
        $ontimeFilter.val(currentYear);
    } else {
        console.warn('On-Time filter element NOT found! Element ID: #ontimeYearFilter');
    }
    
    // Load KPI regardless of filter existence
    setTimeout(function() {
        loadOnTimeSubmissionKpi();
    }, 500);

    // On-Time Year filter change event
    $('#ontimeYearFilter').on('change', function () {
        loadOnTimeSubmissionKpi();
    });

    // Load Contracts Expiring Soon
    loadContractsExpiring(30);
    
    // Contracts expiry filter change event
    $('#expiryDaysFilter').on('change', function () {
        const selectedDays = parseInt($(this).val());
        loadContractsExpiring(selectedDays);
    });

    initVisitorsOverviewChart();
    initSocialRadarChart();
    loadTopVendorChart();
});

// ==============================
// Budget KPI Dashboard Functions
// ==============================

let budgetKpiChart = null;
let monthlyRealisasiChart = null;

// Load Budget Summary
function loadBudgetSummary(year) {
    $.ajax({
        url: `/api/dashboard/budget-summary/${year}`,
        type: 'GET',
        success: function (data) {
            // Update summary cards
            $('#totalBudget').text('Rp ' + Math.round(data.totalBudget).toLocaleString('id-ID'));
            $('#totalRealisasi').text('Rp ' + Math.round(data.totalRealisasi).toLocaleString('id-ID'));
            $('#sisaBudget').text('Rp ' + Math.round(data.totalSisaBudget).toLocaleString('id-ID'));
            $('#persentaseSerapan').text(data.overallPersentaseSerapan.toFixed(2) + '%');

            // Update progress bar
            $('#serapanProgressBar').css('width', data.overallPersentaseSerapan + '%');

            // Update traffic light
            const trafficLight = $('#trafficLight');
            trafficLight.removeClass('bg-success bg-warning bg-danger');
            
            if (data.overallTrafficLight === 'green') {
                trafficLight.addClass('bg-success');
                $('#serapanProgressBar').removeClass('bg-warning bg-danger').addClass('bg-success');
            } else if (data.overallTrafficLight === 'yellow') {
                trafficLight.addClass('bg-warning');
                $('#serapanProgressBar').removeClass('bg-success bg-danger').addClass('bg-warning');
            } else {
                trafficLight.addClass('bg-danger');
                $('#serapanProgressBar').removeClass('bg-success bg-warning').addClass('bg-danger');
            }
        },
        error: function (xhr) {
            console.error('Failed to load budget summary:', xhr);
        }
    });
}

// Load Budget KPI Chart (Budget vs Realisasi by Vendor)
function loadBudgetKpiChart(year) {
    if (!year) year = parseInt($('#budgetYearFilter').val());

    $.ajax({
        url: `/api/dashboard/budget-kpi/${year}`,
        type: 'GET',
        success: function (data) {
            if (data && data.length > 0) {
                const vendorNames = data.map(v => v.vendorName);
                const budgets = data.map(v => v.totalBudget);
                const realisasi = data.map(v => v.realisasi);

                const chartOptions = {
                    series: [
                        {
                            name: 'Budget',
                            data: budgets
                        },
                        {
                            name: 'Realisasi',
                            data: realisasi
                        }
                    ],
                    chart: {
                        type: 'bar',
                        height: 400,
                        toolbar: {
                            show: true
                        }
                    },
                    colors: ['#3b76ef', '#10b981'],
                    plotOptions: {
                        bar: {
                            horizontal: false,
                            columnWidth: '55%',
                            borderRadius: 5,
                            dataLabels: {
                                position: 'top'
                            }
                        }
                    },
                    dataLabels: {
                        enabled: true,
                        formatter: function (val) {
                            return 'Rp ' + (val / 1000000).toFixed(1) + 'M';
                        },
                        offsetY: -20,
                        style: {
                            fontSize: '10px',
                            colors: ['#304758']
                        }
                    },
                    stroke: {
                        show: true,
                        width: 2,
                        colors: ['transparent']
                    },
                    xaxis: {
                        categories: vendorNames,
                        labels: {
                            rotate: -45,
                            rotateAlways: true
                        }
                    },
                    yaxis: {
                        title: {
                            text: 'Amount (Rupiah)'
                        },
                        labels: {
                            formatter: function (val) {
                                return 'Rp ' + (val / 1000000).toFixed(0) + 'M';
                            }
                        }
                    },
                    fill: {
                        opacity: 1
                    },
                    tooltip: {
                        theme: 'light',
                        y: {
                            formatter: function (val) {
                                return 'Rp ' + val.toLocaleString('id-ID');
                            }
                        }
                    },
                    legend: {
                        position: 'top',
                        horizontalAlign: 'right'
                    }
                };

                if (budgetKpiChart) {
                    budgetKpiChart.destroy();
                }

                budgetKpiChart = new ApexCharts(document.querySelector("#budget-kpi-chart"), chartOptions);
                budgetKpiChart.render();
            } else {
                $('#budget-kpi-chart').html('<div class="text-center py-5 text-muted">No data available for selected year</div>');
            }
        },
        error: function (xhr) {
            console.error('Failed to load budget KPI:', xhr);
            $('#budget-kpi-chart').html('<div class="text-center py-5 text-danger">Failed to load data</div>');
        }
    });
}

// Load Monthly Realisasi Trend Chart
function loadMonthlyRealisasiChart(year) {
    if (!year) year = parseInt($('#budgetYearFilter').val());

    $.ajax({
        url: `/api/dashboard/monthly-realisasi/${year}`,
        type: 'GET',
        success: function (data) {
            if (data && data.length > 0) {
                const months = data.map(m => m.monthName);
                const realisasi = data.map(m => m.realisasi);
                const budgets = data.map(m => m.budget);

                const chartOptions = {
                    series: [
                        {
                            name: 'Realisasi',
                            data: realisasi
                        },
                        {
                            name: 'Budget per Bulan',
                            data: budgets
                        }
                    ],
                    chart: {
                        type: 'line',
                        height: 400,
                        toolbar: {
                            show: false
                        }
                    },
                    colors: ['#10b981', '#ef4444'],
                    stroke: {
                        width: [3, 2],
                        curve: 'smooth',
                        dashArray: [0, 5]
                    },
                    markers: {
                        size: 5,
                        colors: ['#10b981', '#ef4444'],
                        strokeColors: '#fff',
                        strokeWidth: 2,
                        hover: {
                            size: 7
                        }
                    },
                    xaxis: {
                        categories: months
                    },
                    yaxis: {
                        title: {
                            text: 'Amount (Rupiah)'
                        },
                        labels: {
                            formatter: function (val) {
                                return 'Rp ' + (val / 1000000).toFixed(0) + 'M';
                            }
                        }
                    },
                    tooltip: {
                        theme: 'light',
                        y: {
                            formatter: function (val) {
                                return 'Rp ' + val.toLocaleString('id-ID');
                            }
                        }
                    },
                    legend: {
                        position: 'top',
                        horizontalAlign: 'right'
                    }
                };

                if (monthlyRealisasiChart) {
                    monthlyRealisasiChart.destroy();
                }

                monthlyRealisasiChart = new ApexCharts(document.querySelector("#monthly-realisasi-chart"), chartOptions);
                monthlyRealisasiChart.render();
            } else {
                $('#monthly-realisasi-chart').html('<div class="text-center py-5 text-muted">No data available</div>');
            }
        },
        error: function (xhr) {
            console.error('Failed to load monthly realisasi:', xhr);
            $('#monthly-realisasi-chart').html('<div class="text-center py-5 text-danger">Failed to load data</div>');
        }
    });
}

// ==============================
// Load Dashboard Statistics
// ==============================
function loadDashboardStats() {
    const token = localStorage.getItem('token');
    
    $.ajax({
        url: '/api/dashboard/stats',
        type: 'GET',
        headers: {
            'Authorization': 'Bearer ' + token
        },
        success: function (data) {
            // Update Active Contracts
            $('#activeContractsCount').text(data.activeContractsCount);
            
            // Update Contracts Expiring Soon with warning if > 0
            $('#contractsExpiringSoon').text(data.contractsExpiringSoon);
            if (data.contractsExpiringSoon > 0) {
                $('#contractsExpiringSoon').addClass('text-warning');
            } else {
                $('#contractsExpiringSoon').removeClass('text-warning').addClass('text-success');
            }
            
            // Update Total Invoices
            $('#totalInvoicesSubmitted').text(data.totalInvoicesSubmitted);
            
            // Update Total Invoice Amount
            const formattedAmount = 'Rp ' + (data.totalInvoiceAmount / 1000000).toFixed(2) + 'M';
            $('#totalInvoiceAmount').text(formattedAmount);
        },
        error: function (xhr) {
            console.error('Failed to load dashboard stats:', xhr);
            $('#activeContractsCount').text('0');
            $('#contractsExpiringSoon').text('0');
            $('#totalInvoicesSubmitted').text('0');
            $('#totalInvoiceAmount').text('Rp 0');
        }
    });
}

// ==============================
// Vendor On-Time Submission KPI
// ==============================
let ontimeSubmissionChart = null;

function loadOnTimeSubmissionKpi() {
    // Get year from filter, or use current year as fallback
    const $yearFilter = $('#ontimeYearFilter');
    let year = null;
    
    if ($yearFilter.length > 0) {
        year = $yearFilter.val();
    } else {
        console.warn('Filter element not found, using default (all years)');
    }
    
    const url = year ? `/api/dashboard/vendor-ontime-submission?year=${year}` : '/api/dashboard/vendor-ontime-submission';
    
    // Check if table element exists
    const $tbody = $('#vendorPerformanceTable');
    if ($tbody.length === 0) {
        console.error('ERROR: Table element #vendorPerformanceTable NOT FOUND in DOM!');
        return;
    }
    
    $.ajax({
        url: url,
        type: 'GET',
        success: function (data) {
            // Update overall stats
            $('#ontimeTotalInvoices').text(data.totalInvoices || 0);
            $('#ontimeOnTimeCount').text(data.onTimeSubmissions || 0);
            $('#ontimeLateCount').text(data.lateSubmissions || 0);
            $('#ontimePercentage').text((data.onTimePercentage || 0).toFixed(2) + '%');
            
            // Update progress bar
            const percentage = data.onTimePercentage || 0;
            $('#ontimeProgressBar').css('width', percentage + '%');
            
            // Set color and badge based on performance
            let progressBarClass = 'bg-success';
            let badgeText = 'Excellent';
            let badgeClass = 'bg-success';
            
            if (percentage < 70) {
                progressBarClass = 'bg-danger';
                badgeText = 'Poor';
                badgeClass = 'bg-danger';
            } else if (percentage < 90) {
                progressBarClass = 'bg-warning';
                badgeText = 'Good';
                badgeClass = 'bg-warning';
            }
            
            $('#ontimeProgressBar').removeClass('bg-success bg-warning bg-danger').addClass(progressBarClass);
            $('#ontimeStatusBadge').removeClass('bg-success bg-warning bg-danger').addClass(badgeClass).text(badgeText);
            
            // Render donut chart
            renderOnTimeChart(data);
            
            // Render vendor performance table
            renderVendorPerformanceTable(data.vendorBreakdown);
        },
        error: function (xhr, status, error) {
            console.error('Failed to load on-time submission KPI:', xhr, status, error);
            console.error('Response:', xhr.responseText);
            
            $('#ontimeTotalInvoices').text('0');
            $('#ontimeOnTimeCount').text('0');
            $('#ontimeLateCount').text('0');
            $('#ontimePercentage').text('0%');
            
            // Show error in table
            const $tbody = $('#vendorPerformanceTable');
            $tbody.empty();
            $tbody.append(`<tr><td colspan="4" class="text-center text-danger py-4">Failed to load data: ${error || 'Unknown error'}</td></tr>`);
        }
    });
}

function renderOnTimeChart(data) {
    const chartOptions = {
        series: [data.onTimeSubmissions, data.lateSubmissions],
        chart: {
            type: 'donut',
            height: 350
        },
        labels: ['On-Time', 'Late'],
        colors: ['#10b981', '#ef4444'],
        legend: {
            position: 'bottom',
            fontSize: '14px'
        },
        plotOptions: {
            pie: {
                donut: {
                    size: '65%',
                    labels: {
                        show: true,
                        name: {
                            show: true,
                            fontSize: '18px',
                            fontWeight: 600,
                            offsetY: -10
                        },
                        value: {
                            show: true,
                            fontSize: '24px',
                            fontWeight: 700,
                            offsetY: 5,
                            formatter: function (val) {
                                return val;
                            }
                        },
                        total: {
                            show: true,
                            label: 'Total Invoices',
                            fontSize: '14px',
                            fontWeight: 400,
                            color: '#9ca3af',
                            formatter: function (w) {
                                return w.globals.seriesTotals.reduce((a, b) => {
                                    return a + b;
                                }, 0);
                            }
                        }
                    }
                }
            }
        },
        dataLabels: {
            enabled: true,
            formatter: function (val, opts) {
                return opts.w.config.series[opts.seriesIndex];
            },
            style: {
                fontSize: '14px',
                fontWeight: 600
            }
        },
        tooltip: {
            theme: 'light',
            y: {
                formatter: function (val, opts) {
                    const percentage = ((val / data.totalInvoices) * 100).toFixed(1);
                    return val + ' (' + percentage + '%)';
                }
            }
        }
    };
    
    if (ontimeSubmissionChart) {
        ontimeSubmissionChart.destroy();
    }
    
    ontimeSubmissionChart = new ApexCharts(document.querySelector("#ontime-submission-chart"), chartOptions);
    ontimeSubmissionChart.render();
}

function renderVendorPerformanceTable(vendors) {
    
    const $tbody = $('#vendorPerformanceTable');
    
    $tbody.empty();
    
    if (!vendors || vendors.length === 0) {
        $tbody.append('<tr><td colspan="4" class="text-center text-muted py-4">No vendor data available</td></tr>');
        return;
    }
    
    
    vendors.forEach((vendor, index) => {
        const percentage = vendor.onTimePercentage.toFixed(1);
        
        // Determine badge color based on performance
        let badgeClass = 'bg-success';
        let badgeText = vendor.performanceStatus;
        
        if (vendor.performanceStatus === 'Poor') {
            badgeClass = 'bg-danger';
        } else if (vendor.performanceStatus === 'Good') {
            badgeClass = 'bg-warning';
        }
        
        // Add rank badge for top 3
        let rankBadge = '';
        if (index === 0) {
            rankBadge = '<i class="feather-award text-warning me-2"></i>';
        } else if (index === 1) {
            rankBadge = '<i class="feather-award text-muted me-2"></i>';
        } else if (index === 2) {
            rankBadge = '<i class="feather-award text-secondary me-2"></i>';
        }
        
        const row = `
            <tr>
                <td>
                    ${rankBadge}<strong>${vendor.vendorName}</strong>
                    <div class="fs-11 text-muted">${vendor.onTimeSubmissions} on-time / ${vendor.lateSubmissions} late</div>
                </td>
                <td class="text-center">${vendor.totalInvoices}</td>
                <td class="text-center">
                    <div class="d-flex align-items-center justify-content-center gap-2">
                        <span class="fw-bold">${percentage}%</span>
                        <div class="progress" style="width: 80px; height: 8px;">
                            <div class="progress-bar ${badgeClass}" role="progressbar" style="width: ${percentage}%"></div>
                        </div>
                    </div>
                </td>
                <td class="text-center">
                    <span class="badge ${badgeClass}">${badgeText}</span>
                </td>
            </tr>
        `;
        
        $tbody.append(row);
    });
}
// ==============================
// Contracts Expiring Soon
// ==============================

let contractsData = [];
let contractsCurrentPage = 1;
const contractsPerPage = 5;

function loadContractsExpiring(days = 30) {
    //console.log('Loading contracts expiring in', days, 'days');
    $.ajax({
        url: `/api/dashboard/contracts-expiring?days=${days}`,
        method: 'GET',
        success: function(response) {
            //console.log('Contracts response:', response);
            if (response.success) {
                contractsData = response.data || [];
                contractsCurrentPage = 1;
                updateContractsExpiringTable();
                updateContractsPagination();
                updateContractSummary();
                $('#expiringContractsCount').text(response.count);
            } else {
                console.error('Response success is false:', response);
                $('#contractsExpiringTableBody').html(
                    '<tr><td colspan="4" class="text-center text-danger py-4">Failed to load contracts: ' + (response.message || 'Unknown error') + '</td></tr>'
                );
            }
        },
        error: function(xhr, status, error) {
            console.error('AJAX error:', { xhr, status, error });
            console.error('Response text:', xhr.responseText);
            let errorMessage = 'Failed to load contracts';
            if (xhr.status === 401) {
                errorMessage = 'Unauthorized - Please login again';
            } else if (xhr.status === 500) {
                try {
                    const errorData = JSON.parse(xhr.responseText);
                    errorMessage = errorData.message || errorMessage;
                } catch (e) {
                    errorMessage = xhr.responseText || errorMessage;
                }
            }
            $('#contractsExpiringTableBody').html(
                '<tr><td colspan="4" class="text-center text-danger py-4">' + errorMessage + '</td></tr>'
            );
        }
    });
}

function updateContractsExpiringTable() {
    //console.log('Updating table with contracts:', contractsData);
    const $tbody = $('#contractsExpiringTableBody');
    $tbody.empty();
    
    if (!contractsData || contractsData.length === 0) {
        $tbody.append('<tr><td colspan="4" class="text-center text-muted py-4">No contracts expiring soon</td></tr>');
        $('#expiringContractsCount').text('0');
        return;
    }
    
    // Calculate pagination
    const startIndex = (contractsCurrentPage - 1) * contractsPerPage;
    const endIndex = startIndex + contractsPerPage;
    const paginatedContracts = contractsData.slice(startIndex, endIndex);
    
    paginatedContracts.forEach((contract) => {
        // Determine alert badge
        let alertBadge = '';
        let statusClass = '';
        
        if (contract.alertLevel === 'Critical') {
            alertBadge = '<span class="badge bg-danger">Critical</span>';
            statusClass = 'text-danger fw-bold';
        } else if (contract.alertLevel === 'Warning') {
            alertBadge = '<span class="badge bg-warning">Warning</span>';
            statusClass = 'text-warning fw-bold';
        } else {
            alertBadge = '<span class="badge bg-success">Safe</span>';
            statusClass = 'text-success';
        }
        
        // Format countdown
        const countdownHtml = formatContractCountdown(contract.daysRemaining);
        
        // Format dates
        const endDate = new Date(contract.endDate).toLocaleDateString('id-ID', {
            day: 'numeric',
            month: 'short',
            year: 'numeric'
        });
        
        const row = `
            <tr>
                <td>
                    <div class="hstack gap-2">
                        <span class="wd-10 ht-10 ${contract.alertLevel === 'Critical' ? 'bg-danger' : contract.alertLevel === 'Warning' ? 'bg-warning' : 'bg-success'} rounded-circle d-inline-block me-2 lh-base"></span>
                        <div class="border-3 border-start rounded ps-3">
                            <a href="/Contract/Details/${contract.contractId}" class="mb-2 d-block">
                                <span>${contract.contractNo}</span>
                            </a>
                            <p class="fs-12 text-muted mb-0">${contract.vendorName}</p>
                        </div>
                    </div>
                </td>
                <td>${alertBadge}</td>
                <td>
                    <div class="${statusClass}">
                        ${countdownHtml}
                    </div>
                    <div class="fs-11 text-muted">Ends: ${endDate}</div>
                </td>
                <td class="text-end">
                    <a href="/Contract/Details/${contract.contractId}" class="avatar-text avatar-md ms-auto" data-bs-toggle="tooltip" title="View Details">
                        <i class="feather-arrow-right"></i>
                    </a>
                </td>
            </tr>
        `;
        
        $tbody.append(row);
    });
    
    // Re-initialize tooltips
    $('[data-bs-toggle="tooltip"]').tooltip();
}

function updateContractsPagination() {
    const totalPages = Math.ceil(contractsData.length / contractsPerPage);
    const $pagination = $('#contractsPagination');
    
    if (totalPages <= 1) {
        $pagination.hide();
        return;
    }
    
    $pagination.show();
    $pagination.empty();
    
    // Previous button
    $pagination.append(`
        <li>
            <a href="javascript:void(0);" id="contractsPrevPage" ${contractsCurrentPage === 1 ? 'class="disabled"' : ''}>
                <i class="bi bi-arrow-left"></i>
            </a>
        </li>
    `);
    
    // Page numbers
    let startPage = Math.max(1, contractsCurrentPage - 1);
    let endPage = Math.min(totalPages, startPage + 2);
    
    if (endPage - startPage < 2) {
        startPage = Math.max(1, endPage - 2);
    }
    
    if (startPage > 1) {
        $pagination.append(`<li><a href="javascript:void(0);" data-page="1">1</a></li>`);
        if (startPage > 2) {
            $pagination.append(`<li><a href="javascript:void(0);"><i class="bi bi-dot"></i></a></li>`);
        }
    }
    
    for (let i = startPage; i <= endPage; i++) {
        $pagination.append(`
            <li>
                <a href="javascript:void(0);" data-page="${i}" class="${i === contractsCurrentPage ? 'active' : ''}">${i}</a>
            </li>
        `);
    }
    
    if (endPage < totalPages) {
        if (endPage < totalPages - 1) {
            $pagination.append(`<li><a href="javascript:void(0);"><i class="bi bi-dot"></i></a></li>`);
        }
        $pagination.append(`<li><a href="javascript:void(0);" data-page="${totalPages}">${totalPages}</a></li>`);
    }
    
    // Next button
    $pagination.append(`
        <li>
            <a href="javascript:void(0);" id="contractsNextPage" ${contractsCurrentPage === totalPages ? 'class="disabled"' : ''}>
                <i class="bi bi-arrow-right"></i>
            </a>
        </li>
    `);
    
    // Bind events
    $pagination.find('a[data-page]').off('click').on('click', function(e) {
        e.preventDefault();
        const page = parseInt($(this).data('page'));
        if (page) {
            contractsCurrentPage = page;
            updateContractsExpiringTable();
            updateContractsPagination();
        }
    });
    
    $('#contractsPrevPage').off('click').on('click', function(e) {
        e.preventDefault();
        if (contractsCurrentPage > 1) {
            contractsCurrentPage--;
            updateContractsExpiringTable();
            updateContractsPagination();
        }
    });
    
    $('#contractsNextPage').off('click').on('click', function(e) {
        e.preventDefault();
        if (contractsCurrentPage < totalPages) {
            contractsCurrentPage++;
            updateContractsExpiringTable();
            updateContractsPagination();
        }
    });
}

function formatContractCountdown(days) {
    if (days < 0) {
        return '<span class="text-danger">Expired</span>';
    } else if (days === 0) {
        return '<span class="text-danger fw-bold">Expires Today!</span>';
    } else if (days === 1) {
        return '<span class="text-danger fw-bold">1 Day</span>';
    } else if (days < 7) {
        return `<span class="text-danger fw-bold">${days} Days</span>`;
    } else if (days < 15) {
        return `<span class="text-danger">${days} Days</span>`;
    } else if (days < 30) {
        return `<span class="text-warning">${days} Days</span>`;
    } else {
        return `<span>${days} Days</span>`;
    }
}

function updateContractSummary() {
    const critical = contractsData.filter(c => c.alertLevel === 'Critical').length;
    const warning = contractsData.filter(c => c.alertLevel === 'Warning').length;
    const safe = contractsData.filter(c => c.alertLevel === 'Safe').length;
    
    $('#criticalContractsCount').text(critical);
    $('#warningContractsCount').text(warning);
    $('#safeContractsCount').text(safe);
}