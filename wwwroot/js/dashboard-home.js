// ==============================
// Dashboard Home Charts
// ==============================

// Visitors Overview Statistics Chart
function initVisitorsOverviewChart() {
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
    // Load Dashboard Statistics (Contracts & Invoices)
    loadDashboardStats();

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
