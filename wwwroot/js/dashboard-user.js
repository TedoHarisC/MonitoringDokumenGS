// ==============================
// User Dashboard JavaScript
// ==============================

let userPerformanceChart = null;
let userMonthlyTrendChart = null;

// Load User Statistics
function loadUserStats() {
    const year = $('#userYearFilter').val();
    const url = year ? `/api/dashboard/vendor-ontime-submission?year=${year}` : '/api/dashboard/vendor-ontime-submission';
    
    console.log('Loading user stats from:', url);
    
    $.ajax({
        url: url,
        type: 'GET',
        success: function (data) {
            console.log('User stats received:', data);
            
            // Since this endpoint returns all vendors, we need to get current user's vendor data
            // For now, we'll use the aggregate data
            // In production, you might want a separate endpoint that returns only current user's data
            
            $('#userTotalInvoices').text(data.totalInvoices || 0);
            $('#userOnTimeCount').text(data.onTimeSubmissions || 0);
            $('#userLateCount').text(data.lateSubmissions || 0);
            $('#userOnTimePercentage').text((data.onTimePercentage || 0).toFixed(2) + '%');
            
            // Update progress bar
            const percentage = data.onTimePercentage || 0;
            $('#userProgressBar').css('width', percentage + '%');
            
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
            
            $('#userProgressBar').removeClass('bg-success bg-warning bg-danger').addClass(progressBarClass);
            $('#userStatusBadge').removeClass('bg-success bg-warning bg-danger').addClass(badgeClass).text(badgeText);
            
            // Render charts
            renderUserPerformanceChart(data);
        },
        error: function (xhr, status, error) {
            console.error('Failed to load user stats:', xhr, status, error);
            $('#userTotalInvoices').text('0');
            $('#userOnTimeCount').text('0');
            $('#userLateCount').text('0');
            $('#userOnTimePercentage').text('0%');
        }
    });
}

// Render User Performance Donut Chart
function renderUserPerformanceChart(data) {
    const chartOptions = {
        series: [data.onTimeSubmissions || 0, data.lateSubmissions || 0],
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
                    const total = data.totalInvoices || 1;
                    const percentage = ((val / total) * 100).toFixed(1);
                    return val + ' (' + percentage + '%)';
                }
            }
        }
    };
    
    if (userPerformanceChart) {
        userPerformanceChart.destroy();
    }
    
    userPerformanceChart = new ApexCharts(document.querySelector("#user-performance-chart"), chartOptions);
    userPerformanceChart.render();
}

// Load Monthly Trend with real data
function loadUserMonthlyTrend() {
    const year = $('#userYearFilter').val() || null;
    
    console.log('Loading user monthly trend for year:', year);
    
    $.ajax({
        url: '/api/dashboard/user-monthly-trend',
        method: 'GET',
        data: { year: year },
        success: function (response) {
            console.log('User monthly trend loaded:', response);
            renderUserMonthlyTrendChart(response);
        },
        error: function (xhr, status, error) {
            console.error('Error loading user monthly trend:', error);
            // Show empty chart on error
            renderUserMonthlyTrendChart({
                months: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
                onTimeData: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
                lateData: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]
            });
        }
    });
}

// Render Monthly Trend Chart
function renderUserMonthlyTrendChart(data) {
    const chartOptions = {
        series: [
            {
                name: 'On-Time',
                data: data.onTimeData
            },
            {
                name: 'Late',
                data: data.lateData
            }
        ],
        chart: {
            type: 'bar',
            height: 350,
            stacked: true,
            toolbar: {
                show: false
            }
        },
        colors: ['#10b981', '#ef4444'],
        plotOptions: {
            bar: {
                horizontal: false,
                columnWidth: '55%',
                borderRadius: 5
            }
        },
        dataLabels: {
            enabled: false
        },
        xaxis: {
            categories: data.months
        },
        yaxis: {
            title: {
                text: 'Number of Invoices'
            }
        },
        legend: {
            position: 'top',
            horizontalAlign: 'right'
        },
        fill: {
            opacity: 1
        },
        tooltip: {
            theme: 'light',
            y: {
                formatter: function (val) {
                    return val + ' invoices';
                }
            }
        }
    };
    
    if (userMonthlyTrendChart) {
        userMonthlyTrendChart.destroy();
    }
    
    userMonthlyTrendChart = new ApexCharts(document.querySelector("#user-monthly-trend-chart"), chartOptions);
    userMonthlyTrendChart.render();
}

// Load Recent Invoices
function loadRecentInvoices() {
    $.ajax({
        url: '/api/invoices',
        type: 'GET',
        success: function (data) {
            console.log('Recent invoices received:', data);
            
            const $tbody = $('#recentInvoicesTable');
            $tbody.empty();
            
            if (!data || data.length === 0) {
                $tbody.append('<tr><td colspan="6" class="text-center text-muted py-4">No invoices yet. Create your first invoice!</td></tr>');
                return;
            }
            
            // Show only last 5 invoices
            const recentInvoices = data.slice(0, 5);
            
            recentInvoices.forEach(function(invoice) {
                const onTimeBadge = invoice.isOnTime 
                    ? '<span class="badge bg-success d-flex align-items-center justify-content-center"><i class="feather-check"></i> On-Time</span>'
                    : '<span class="badge bg-danger d-flex align-items-center justify-content-center"><i class="feather-x"></i> Late</span>';
                
                const statusBadge = `<span class="badge bg-primary">${invoice.progressStatusId}</span>`;
                
                const formattedAmount = 'Rp ' + (invoice.invoiceAmount || 0).toLocaleString('id-ID');
                const formattedDate = new Date(invoice.createdAt).toLocaleDateString('id-ID');
                
                const row = `
                    <tr>
                        <td><strong>${invoice.invoiceNumber}</strong></td>
                        <td>${formattedAmount}</td>
                        <td class="text-center">${statusBadge}</td>
                        <td class="text-center">${onTimeBadge}</td>
                        <td>${formattedDate}</td>
                        <td class="text-center">
                            <a href="/Invoice" class="btn btn-sm btn-light-brand">
                                <i class="feather-eye"></i>
                            </a>
                        </td>
                    </tr>
                `;
                
                $tbody.append(row);
            });
        },
        error: function (xhr) {
            console.error('Failed to load recent invoices:', xhr);
            const $tbody = $('#recentInvoicesTable');
            $tbody.empty();
            $tbody.append('<tr><td colspan="6" class="text-center text-danger py-4">Failed to load invoices</td></tr>');
        }
    });
}

// Initialize on Document Ready
$(document).ready(function () {
    console.log('User Dashboard initializing...');
    
    const currentYear = new Date().getFullYear();
    $('#userYearFilter').val(currentYear);
    
    // Load all data
    setTimeout(function() {
        loadUserStats();
        loadUserMonthlyTrend();
        loadRecentInvoices();
    }, 300);
    
    // Year filter change event
    $('#userYearFilter').on('change', function () {
        console.log('User year filter changed to:', $(this).val());
        loadUserStats();
        loadUserMonthlyTrend();
    });
});
