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

// ==============================
// Uang Muka Detail Modal & Export
// ==============================
function showUangMukaDetailModal(jenis, statusName, statusId) {
  // Set modal title
  $("#uangMukaDetailModalLabel").text(
    `Detail Uang Muka - ${jenis} [${statusName}]`,
  );
  // Show loading
  $("#uangMukaDetailContent").html(
    '<div class="text-center py-5"><div class="spinner-border"></div><div>Loading...</div></div>',
  );
  portalModalToBody("uangMukaDetailModal");
  showModal("uangMukaDetailModal");
  $.ajax({
    url: `/api/dashboard/uang-muka-detail?jenis=${encodeURIComponent(jenis)}&status=${encodeURIComponent(statusId)}`,
    method: "GET",
    success: function (data) {
      if (data && data.length > 0) {
        let html = `<div class="table-responsive"><table id="uangMukaDetailTable" class="table table-bordered table-sm" style="width:100%"><thead><tr>`;
        const columns = Object.keys(data[0]);
        columns.forEach(function (key) {
          html += `<th>${key}</th>`;
        });
        html += "</tr></thead><tbody>";
        data.forEach(function (row) {
          html += "<tr>";
          columns.forEach(function (key) {
            let val = row[key];
            if (key.toLowerCase() === "amount") {
              val =
                val != null ? "Rp " + Number(val).toLocaleString("id-ID") : "";
            } else if (key.toLowerCase().includes("date") && val) {
              const dateObj = new Date(val);
              val = dateObj.toLocaleDateString("id-ID", {
                day: "2-digit",
                month: "long",
                year: "numeric",
              });
            }
            html += `<td>${val ?? ""}</td>`;
          });
          html += "</tr>";
        });
        html += "</tbody></table></div>";
        // Area for export button above table
        html =
          `<div class="d-flex justify-content-start mb-2"><div id="uangMukaExportBtnArea"></div></div>` +
          html;
        $("#uangMukaDetailContent").html(html);

        // Load DataTable & export button
        ensureDataTablesButtons(function () {
          const dt = $("#uangMukaDetailTable").DataTable({
            responsive: true,
            order: [],
            autoWidth: false,
            searching: false,
            paging: false,
            info: false,
            language: {
              emptyTable: "No detail data available",
            },
            dom: "Bfrtip",
            buttons: [
              {
                extend: "excelHtml5",
                text: '<i class="feather-download"></i> Export Excel',
                className: "btn btn-success",
                title: `Detail Uang Muka - ${jenis} [${statusName}]`,
                exportOptions: {
                  columns: ":visible",
                },
                footer: false,
              },
            ],
          });
          // Move export button to custom area above table (top left)
          dt.buttons().container().appendTo("#uangMukaExportBtnArea");
        });
      } else {
        $("#uangMukaDetailContent").html(
          '<div class="text-center text-muted py-4">No detail data available</div>',
        );
      }
    },
    error: function () {
      $("#uangMukaDetailContent").html(
        '<div class="text-center text-muted py-4">No detail data available</div>',
      );
    },
  });
  // Store for export
  $("#uangMukaDetailModal").data("jenis", jenis).data("status", statusName);
}

// Card click event (delegated)
$(document).on("click", ".uangmuka-status-card", function () {
  const jenis = $(this).data("jenis");
  const status = $(this).data("status");
  const statusId = $(this).data("status-id"); // pastikan data-status-id ada di elemen
  showUangMukaDetailModal(jenis, status, statusId);
});

// Hapus handler export manual, export pakai DataTables
// Pastikan DataTables Buttons sudah dimuat
function ensureDataTablesButtons(callback) {
  if ($.fn.dataTable && $.fn.dataTable.Buttons) {
    callback();
    return;
  }
  // Load loader script jika belum ada
  if (!window._dtButtonsLoaderInjected) {
    var s = document.createElement("script");
    s.src = "/js/datatables-buttons-loader.js";
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
// ==============================
// Uang Muka Total by Status (for #uangmuka_by_status_area_*)
// ==============================
function loadUangMukaByStatusArea(jenis, areaId) {
  $.ajax({
    url:
      "/api/dashboard/uang-muka-status-summary?jenis=" +
      encodeURIComponent(jenis),
    method: "GET",
    success: function (data) {
      const area = $("#" + areaId);
      area.empty();
      let statusList = [];
      if (data && data.statusCounts) {
        statusList = data.statusCounts;
      }
      if (!statusList || statusList.length === 0) {
        area.html(
          '<div class="col-12 text-center text-muted py-4">No status data available</div>',
        );
        return;
      }
      let html = '<div class="row justify-content-center">';
      statusList.forEach(function (item) {
        let color = "secondary";
        let icon = "feather-info";
        // Status name mapping (customize as needed)
        switch ((item.statusName || "").toLowerCase()) {
          case "draft":
            color = "secondary";
            icon = "feather-edit";
            break;
          case "submitted":
            color = "info";
            icon = "feather-upload";
            break;
          case "approved":
            color = "primary";
            icon = "feather-thumbs-up";
            break;
          case "done":
            color = "success";
            icon = "feather-check-circle";
            break;
          case "rejected":
            color = "danger";
            icon = "feather-x-circle";
            break;
          case "verifikasi gs":
            color = "info";
            icon = "feather-info";
            break;
          case "validasi & pembayaran fa":
            color = "secondary";
            icon = "feather-dollar-sign";
            break;
          case "biaya selesai":
            color = "success";
            icon = "feather-check-circle";
            break;
        }
        html += `
                <div class="col-xl-2 col-lg-3 col-md-4 col-6 mb-3">
                    <div class="p-3 border border-dashed rounded uangmuka-status-card" 
                         data-status="${item.statusName}" data-jenis="${jenis}" data-status-id="${item.statusId}">
                        <div class="fs-12 text-muted mb-1">${item.statusName}</div>
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
      html += "</div>";
      area.html(html);
    },
    error: function () {
      $("#" + areaId).html(
        '<div class="col-12 text-center text-danger py-4">Failed to load data</div>',
      );
    },
  });
}
// Tab event binding for Uang Muka dashboard
$(document).ready(function () {
  // Initial load
  loadUangMukaByStatusArea("Advanced", "uangmuka_by_status_area_advanced");
  // Tab click events
  $("#tab-advanced").on("click", function () {
    loadUangMukaByStatusArea("Advanced", "uangmuka_by_status_area_advanced");
  });
  $("#tab-biaya").on("click", function () {
    loadUangMukaByStatusArea("Biaya", "uangmuka_by_status_area_biaya");
  });
  $("#tab-realisasi").on("click", function () {
    loadUangMukaByStatusArea("Realisasi", "uangmuka_by_status_area_realisasi");
  });
});

// ==============================
// Invoice Total by Status (for #invoice_by_status_area)
// ==============================
function loadInvoiceByStatusArea() {
  $.ajax({
    url: "/api/dashboard/invoice-status-summary",
    method: "GET",
    success: function (data) {
      const area = $("#invoice_by_status_area");
      area.empty();
      // Ambil semua status dari API (selalu muncul semua status meski 0)
      let statusList = [];
      if (data && data.statusCounts) {
        statusList = data.statusCounts;
      }
      if (!statusList || statusList.length === 0) {
        area.html(
          '<div class="col-12 text-center text-muted py-4">No status data available</div>',
        );
        return;
      }
      // Bungkus card dalam row dan center jika kurang dari 6
      let html = '<div class="row justify-content-center">';
      statusList.forEach(function (item) {
        let color = "secondary";
        let icon = "feather-info";
        switch (item.progressStatusName) {
          case "Verifikasi GS":
            color = "info";
            icon = "feather-info";
            break;
          case "Done":
            color = "success";
            icon = "feather-check-circle";
            break;
          case "Approved by HCGS Dept":
            color = "warning";
            icon = "feather-thumbs-up";
            break;
          case "Approved by KTT":
            color = "primary";
            icon = "feather-thumbs-up";
            break;
          case "Validasi & Pembayaran FA":
            color = "secondary";
            icon = "feather-dollar-sign";
            break;
          case "Approved":
            color = "primary";
            icon = "feather-thumbs-up";
            break;
          case "Rejected":
            color = "danger";
            icon = "feather-x-circle";
            break;
        }

        let progressStatusName =
          item.progressStatusName == "Validasi & Pembayaran FA"
            ? "Pembayaran"
            : item.progressStatusName == "Approved by HCGS Dept"
              ? "Approved by HCGS"
              : item.progressStatusName;
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
      html += "</div>";
      area.html(html);
    },
    error: function () {
      $("#invoice_by_status_area").html(
        '<div class="col-12 text-center text-danger py-4">Failed to load data</div>',
      );
    },
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
    series: [
      {
        name: "Visitors",
        data: [31, 40, 28, 51, 42, 85, 77, 65, 90, 120, 95, 110],
      },
    ],
    chart: {
      type: "area",
      height: 350,
      toolbar: {
        show: false,
      },
    },
    colors: ["#3b76ef"],
    dataLabels: {
      enabled: false,
    },
    stroke: {
      curve: "smooth",
      width: 2,
    },
    xaxis: {
      categories: [
        "Jan",
        "Feb",
        "Mar",
        "Apr",
        "May",
        "Jun",
        "Jul",
        "Aug",
        "Sep",
        "Oct",
        "Nov",
        "Dec",
      ],
    },
    yaxis: {
      title: {
        text: "Visitors",
      },
    },
    fill: {
      type: "gradient",
      gradient: {
        shadeIntensity: 1,
        opacityFrom: 0.7,
        opacityTo: 0.3,
        stops: [0, 90, 100],
      },
    },
    tooltip: {
      theme: "light",
    },
  };

  const visitorsChart = new ApexCharts(
    document.querySelector("#visitors-overview-statistics-chart"),
    visitorsOverviewOptions,
  );
  visitorsChart.render();
}

// Social Radar Chart
function initSocialRadarChart() {
  // Guard: jangan render jika elemen tidak ada di DOM
  if (!document.querySelector("#social-radar-chart")) return;

  const socialRadarOptions = {
    series: [
      {
        name: "Engagement",
        data: [80, 50, 30, 40, 100, 20],
      },
    ],
    chart: {
      height: 350,
      type: "radar",
      toolbar: {
        show: false,
      },
    },
    colors: ["#3b76ef"],
    xaxis: {
      categories: [
        "Facebook",
        "Twitter",
        "Instagram",
        "LinkedIn",
        "YouTube",
        "TikTok",
      ],
    },
    yaxis: {
      show: false,
    },
    fill: {
      opacity: 0.2,
    },
    stroke: {
      show: true,
      width: 2,
      colors: ["#3b76ef"],
      dashArray: 0,
    },
    markers: {
      size: 4,
      colors: ["#3b76ef"],
      strokeColors: "#fff",
      strokeWidth: 2,
    },
  };

  const socialRadarChart = new ApexCharts(
    document.querySelector("#social-radar-chart"),
    socialRadarOptions,
  );
  socialRadarChart.render();
}

// Top 5 Vendor Spend Chart
let topVendorChart = null;

// Load Top Vendor Spend Chart
function loadTopVendorChart() {
  $.ajax({
    url: "/api/dashboard/top-vendors?top=5",
    type: "GET",
    success: function (data) {
      if (data && data.length > 0) {
        const vendorNames = data.map((v) => v.vendorName);
        const vendorSpends = data.map((v) => v.totalSpend);

        const topVendorOptions = {
          series: [
            {
              name: "Total Spend",
              data: vendorSpends,
            },
          ],
          chart: {
            type: "bar",
            height: 350,
            toolbar: {
              show: false,
            },
          },
          colors: ["#3b76ef"],
          plotOptions: {
            bar: {
              horizontal: true,
              borderRadius: 4,
              dataLabels: {
                position: "top",
              },
            },
          },
          dataLabels: {
            enabled: true,
            formatter: function (val) {
              return "Rp " + val.toLocaleString("id-ID");
            },
            offsetX: -6,
            style: {
              fontSize: "11px",
              colors: ["#fff"],
            },
          },
          xaxis: {
            categories: vendorNames,
            labels: {
              formatter: function (val) {
                return "Rp " + (val / 1000000).toFixed(1) + "M";
              },
            },
          },
          yaxis: {
            title: {
              text: "Vendor",
            },
          },
          tooltip: {
            theme: "light",
            y: {
              formatter: function (val) {
                return "Rp " + val.toLocaleString("id-ID");
              },
            },
          },
        };

        // Destroy existing chart if any
        if (topVendorChart) {
          topVendorChart.destroy();
        }

        topVendorChart = new ApexCharts(
          document.querySelector("#top-vendor-spend-chart"),
          topVendorOptions,
        );
        topVendorChart.render();
      } else {
        $("#top-vendor-spend-chart").html(
          '<div class="text-center py-5 text-muted">No vendor data available</div>',
        );
      }
    },
    error: function (xhr) {
      console.error("Failed to load top vendors:", xhr);
      $("#top-vendor-spend-chart").html(
        '<div class="text-center py-5 text-danger">Failed to load data</div>',
      );
    },
  });
}

// Initialize all charts on page load
$(document).ready(function () {
  // Bind click event for dynamically rendered status cards (delegated)
  $(document).on("click", ".invoice-status-card", function () {
    const status = $(this).data("status");
    showInvoicesByStatusModal(status);
  });

  // Saat modal terbuka: fokus ke modal-body bukan close button
  $("#invoiceStatusModal").on("shown.bs.modal", function () {
    $(this).find(".modal-body").attr("tabindex", "-1").trigger("focus");
  });

  // Saat modal AKAN ditutup: blur dulu sebelum aria-hidden ditambahkan
  $("#invoiceStatusModal").on("hide.bs.modal", function () {
    if (document.activeElement) {
      document.activeElement.blur();
    }
  });

  // Saat modal sudah tertutup: cleanup DataTable
  $("#invoiceStatusModal").on("hidden.bs.modal", function () {
    if ($.fn.DataTable.isDataTable("#modalInvoicesTable")) {
      $("#modalInvoicesTable").DataTable().clear().destroy();
    }
    $("#modalInvoicesTable tbody").empty();
  });
});

// Show modal and load invoices by status
function showInvoicesByStatusModal(status) {
  // Set modal title
  $("#invoiceStatusModalLabel").text("Invoice List - " + status);
  // Show modal
  var modal = new bootstrap.Modal(
    document.getElementById("invoiceStatusModal"),
  );
  modal.show();

  // Fetch invoices by status
  $.ajax({
    url: "/api/invoices/filter-by-status?status=" + encodeURIComponent(status),
    method: "GET",
    success: function (data) {
      let rows = "";
      let isEmpty = !data || data.length === 0;

      //console.log(data);

      if (!isEmpty) {
        const monthNames = [
          "",
          "Jan",
          "Feb",
          "Mar",
          "Apr",
          "May",
          "Jun",
          "Jul",
          "Aug",
          "Sep",
          "Oct",
          "Nov",
          "Dec",
        ];
        data.forEach(function (inv) {
          // Pastikan field yang dipakai sesuai API
          const invoiceAmount =
            inv.invoiceAmount !== undefined && inv.invoiceAmount !== null
              ? inv.invoiceAmount.toLocaleString("id-ID", {
                  style: "currency",
                  currency: "IDR",
                })
              : "";
          const taxAmount =
            inv.taxAmount !== undefined && inv.taxAmount !== null
              ? inv.taxAmount.toLocaleString("id-ID", {
                  style: "currency",
                  currency: "IDR",
                })
              : "";
          const grandTotal =
            inv.grandTotal !== undefined && inv.grandTotal !== null
              ? inv.grandTotal.toLocaleString("id-ID", {
                  style: "currency",
                  currency: "IDR",
                })
              : "";
          const year = inv.invoiceYear || "";
          const monthNum = inv.invoiceMonth || 0;
          const month = monthNames[monthNum] || inv.invoiceMonth || "";
          rows += `<tr>
                            <td>${inv.invoiceNumber || ""}</td>
                            <td>${inv.vendorName || ""}</td>
                            <td>${inv.progressStatusName || ""}</td>
                            <td class="text-end">${grandTotal}</td>
                            <td class="text-end">${inv.noSAP}</td>
                            <td class="text-center">${year}</td>
                            <td class="text-center">${month}</td>
                            <td class="text-center">${inv.isOnTime ? "On Time" : "Late"}</td>
                            <td>${inv.createdAt ? new Date(inv.createdAt).toLocaleString("id-ID") : ""}</td>
                        </tr>`;
        });
      }

      // Destroy DataTable if exists
      if ($.fn.DataTable.isDataTable("#modalInvoicesTable")) {
        $("#modalInvoicesTable").DataTable().destroy();
      }

      // Kosongkan tbody, lalu isi jika ada data
      $("#modalInvoicesTable tbody").html(isEmpty ? "" : rows);

      // Pastikan DataTables Buttons sudah dimuat
      function ensureDataTablesButtons(callback) {
        if ($.fn.dataTable && $.fn.dataTable.Buttons) {
          callback();
          return;
        }
        // Load loader script jika belum ada
        if (!window._dtButtonsLoaderInjected) {
          var s = document.createElement("script");
          s.src = "/js/datatables-buttons-loader.js";
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

      ensureDataTablesButtons(function () {
        $("#modalInvoicesTable").DataTable({
          responsive: true,
          order: [],
          autoWidth: false,
          searching: false,
          paging: false,
          info: false,
          language: {
            emptyTable: "No invoices found for this status.",
          },
          dom: "Bfrtip",
          buttons: [
            {
              extend: "excelHtml5",
              text: '<i class="feather-download"></i> Export Excel',
              className: "btn btn-success btn-sm",
              title: "Invoice List - " + status,
              exportOptions: {
                columns: ":visible:not(:last-child)",
              },
            },
          ],
        });
      });
    },
    error: function () {
      if ($.fn.DataTable.isDataTable("#modalInvoicesTable")) {
        $("#modalInvoicesTable").DataTable().destroy();
      }
      $("#modalInvoicesTable tbody").html("");
      $("#modalInvoicesTable").DataTable({
        responsive: true,
        order: [],
        autoWidth: false,
        searching: false,
        paging: false,
        info: false,
        language: {
          emptyTable: "Failed to load invoices.",
        },
      });
    },
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
  $("#budgetYearFilter").val(currentYear);
  $("#budget-year-display").text(currentYear);
  loadBudgetSummary(currentYear);
  initBudgetCoaFilters(currentYear);
  loadBudgetCoaChart(currentYear);
  loadMonthlyRealisasiChart(currentYear);

  // Year filter change event
  $("#budgetYearFilter").on("change", function () {
    const selectedYear = parseInt($(this).val());
    $("#budget-year-display").text(selectedYear);
    loadBudgetSummary(selectedYear);
    initBudgetCoaFilters(selectedYear);
    loadBudgetCoaChart(selectedYear);
    loadMonthlyRealisasiChart(selectedYear);
  });

  // Budget Summary single filters
  $("#budgetSummaryBudgetCodeFilter").on("change", function () {
    const budgetCodeId = $(this).val() || "";
    loadBudgetSummaryCOAOptions(budgetCodeId);
    const year = parseInt($("#budgetYearFilter").val());
    const vcId = "";
    $("#budgetSummaryCOAFilter").val("");
    loadBudgetSummary(year);
  });

  $("#budgetSummaryCOAFilter").on("change", function () {
    const year = parseInt($("#budgetYearFilter").val());
    loadBudgetSummary(year);
  });

  $("#budgetCoaBudgetCodeFilter").on("change", function () {
    const selectedBudgetCodeIds = ($(this).val() || []).filter(Boolean);
    renderSelectedBadges(
      "budgetCoaBudgetCodeFilter",
      "budgetCodeSelectedBadges",
      budgetCodeOptionMap,
      "bg-primary-subtle text-primary",
    );
    loadBudgetCoaTextOptions(selectedBudgetCodeIds);
  });

  $("#budgetCoaTextFilter").on("change", function () {
    renderSelectedBadges(
      "budgetCoaTextFilter",
      "coaTextSelectedBadges",
      coaTextOptionMap,
      "bg-success-subtle text-success",
    );
  });

  $("#btnBudgetCoaApply").on("click", function () {
    const selectedYear = parseInt($("#budgetYearFilter").val());
    loadBudgetCoaChart(selectedYear);
  });

  $("#btnBudgetCoaReset").on("click", function () {
    const selectedYear = parseInt($("#budgetYearFilter").val());
    initBudgetCoaFilters(selectedYear);
    loadBudgetCoaChart(selectedYear);
  });

  $("#btnExportBudgetCoaPng").on("click", function () {
    exportBudgetCoaAsPng();
  });

  $("#btnExportBudgetCoaCsv").on("click", function () {
    exportBudgetCoaAsCsv();
  });

  // Load Vendor On-Time Submission KPI
  const $ontimeFilter = $("#ontimeYearFilter");
  if ($ontimeFilter.length > 0) {
    $ontimeFilter.val(currentYear);
  } else {
    console.warn(
      "On-Time filter element NOT found! Element ID: #ontimeYearFilter",
    );
  }

  // Load KPI regardless of filter existence
  setTimeout(function () {
    loadOnTimeSubmissionKpi();
  }, 500);

  // On-Time Year filter change event
  $("#ontimeYearFilter").on("change", function () {
    loadOnTimeSubmissionKpi();
  });

  // Load Contracts Expiring Soon
  loadContractsExpiring(30);

  // Contracts expiry filter change event
  $("#expiryDaysFilter").on("change", function () {
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

let budgetCoaChart = null;
let budgetCoaRows = [];
let monthlyRealisasiChart = null;
let budgetCodeOptionMap = {};
let coaTextOptionMap = {};

const rupiahCompactFormatter = new Intl.NumberFormat("id-ID", {
  style: "currency",
  currency: "IDR",
  notation: "compact",
  maximumFractionDigits: 1,
});

const rupiahFormatter = new Intl.NumberFormat("id-ID", {
  style: "currency",
  currency: "IDR",
  maximumFractionDigits: 0,
});

function formatRupiahCompact(value) {
  const amount = Number(value || 0);
  return rupiahCompactFormatter.format(amount);
}

function formatRupiah(value) {
  const amount = Number(value || 0);
  return rupiahFormatter.format(amount);
}

function wrapLabelTwoLines(value, maxCharsPerLine = 34) {
  const text = String(value || "").trim();
  if (!text) return [""];
  if (text.length <= maxCharsPerLine) return [text];

  const words = text.split(/\s+/);
  let line1 = "";
  let line2 = "";

  words.forEach((word) => {
    const line1Candidate = line1 ? `${line1} ${word}` : word;
    if (line1Candidate.length <= maxCharsPerLine) {
      line1 = line1Candidate;
      return;
    }

    const line2Candidate = line2 ? `${line2} ${word}` : word;
    line2 = line2Candidate;
  });

  if (!line2) {
    line2 = text.slice(maxCharsPerLine);
  }

  if (line2.length > maxCharsPerLine) {
    line2 = `${line2.slice(0, maxCharsPerLine - 1)}...`;
  }

  return [line1, line2];
}

function renderSelectedBadges(selectId, containerId, optionMap, badgeClass) {
  const selected = ($(`#${selectId}`).val() || []).map(String);
  const $container = $(`#${containerId}`);
  $container.empty();

  if (!selected.length) {
    $container.html('<span class="text-muted small">No selection</span>');
    return;
  }

  selected.forEach((value) => {
    const label = optionMap[value] || value;
    $container.append(
      `<span class="badge ${badgeClass}"><span class="budget-filter-badge-text">${label}</span><button type="button" class="budget-filter-remove" data-select="${selectId}" data-value="${value}" aria-label="Remove">x</button></span>`,
    );
  });
}

$(document).on("click", ".budget-filter-remove", function () {
  const selectId = $(this).data("select");
  const value = String($(this).data("value"));
  const $select = $(`#${selectId}`);
  const current = ($select.val() || []).map(String);
  const next = current.filter((x) => x !== value);
  $select.val(next).trigger("change");
});

function initBudgetCoaSelect2() {
  const $budget = $("#budgetCoaBudgetCodeFilter");
  const $coa = $("#budgetCoaTextFilter");

  if ($budget.length && $.fn.select2) {
    if ($budget.hasClass("select2-hidden-accessible")) {
      $budget.select2("destroy");
    }
    $budget.select2({
      theme: "bootstrap-5",
      placeholder: "Select one or more Budget Code",
      width: "100%",
      closeOnSelect: false,
      allowClear: true,
    });
    renderSelectedBadges(
      "budgetCoaBudgetCodeFilter",
      "budgetCodeSelectedBadges",
      budgetCodeOptionMap,
      "bg-primary-subtle text-primary",
    );
  }

  if ($coa.length && $.fn.select2) {
    if ($coa.hasClass("select2-hidden-accessible")) {
      $coa.select2("destroy");
    }
    $coa.select2({
      theme: "bootstrap-5",
      placeholder: "Select one or more COA Text",
      width: "100%",
      closeOnSelect: false,
      allowClear: true,
    });
    renderSelectedBadges(
      "budgetCoaTextFilter",
      "coaTextSelectedBadges",
      coaTextOptionMap,
      "bg-success-subtle text-success",
    );
  }
}

function initBudgetCoaFilters(year) {
  const effectiveYear =
    year || parseInt($("#budgetYearFilter").val()) || new Date().getFullYear();
  $("#budgetCoaStartDate").val(`${effectiveYear}-01-01`);
  $("#budgetCoaEndDate").val(`${effectiveYear}-12-31`);
  $("#budgetCoaBudgetCodeFilter").val([]).trigger("change");
  $("#budgetCoaTextFilter").html("").prop("disabled", true);
  coaTextOptionMap = {};
  renderSelectedBadges(
    "budgetCoaBudgetCodeFilter",
    "budgetCodeSelectedBadges",
    budgetCodeOptionMap,
    "bg-primary-subtle text-primary",
  );
  renderSelectedBadges(
    "budgetCoaTextFilter",
    "coaTextSelectedBadges",
    coaTextOptionMap,
    "bg-success-subtle text-success",
  );
  loadBudgetCodeOptions();
}

function loadBudgetCodeOptions() {
  $.ajax({
    url: "/api/budget-codes?page=1&pageSize=2000",
    type: "GET",
    success: function (result) {
      const rows = Array.isArray(result)
        ? result
        : result?.items || result?.data || [];
      const $select = $("#budgetCoaBudgetCodeFilter");
      $select.html("");
      budgetCodeOptionMap = {};

      // Reset Budget Summary filter too
      const $sumSel = $("#budgetSummaryBudgetCodeFilter");
      if ($sumSel.length) {
        $sumSel.find("option:not([value=''])").remove();
      }

      rows.forEach(function (item) {
        const id = item.budgetCodeId || item.BudgetCodeId;
        const code = item.code || item.Code || "";
        const desc = item.description || item.Description || "";
        const label = desc ? `${code} - ${desc}` : code;
        budgetCodeOptionMap[String(id)] = label;
        $select.append(`<option value="${id}">${label}</option>`);

        // Also populate Budget Summary single-select filter
        if ($sumSel.length) {
          $sumSel.append(`<option value="${id}">${label}</option>`);
        }
      });

      initBudgetCoaSelect2();
    },
    error: function () {
      $("#budgetCoaBudgetCodeFilter").html("");
      budgetCodeOptionMap = {};
      initBudgetCoaSelect2();
    },
  });
}

function loadBudgetCoaTextOptions(budgetCodeIds) {
  const $coa = $("#budgetCoaTextFilter");
  const previousSelected = ($coa.val() || []).map(String);
  const ids = Array.isArray(budgetCodeIds)
    ? budgetCodeIds.filter(Boolean)
    : budgetCodeIds
      ? [budgetCodeIds]
      : [];

  if (!ids.length) {
    $coa.html("").prop("disabled", true).val([]).trigger("change");
    coaTextOptionMap = {};
    renderSelectedBadges(
      "budgetCoaTextFilter",
      "coaTextSelectedBadges",
      coaTextOptionMap,
      "bg-success-subtle text-success",
    );
    return;
  }

  $.ajax({
    url: `/api/vendor-categories/by-budget-codes?ids=${encodeURIComponent(ids.join(","))}`,
    type: "GET",
    success: function (rows) {
      const list = Array.isArray(rows) ? rows : [];
      $coa.html("");
      coaTextOptionMap = {};

      list.forEach(function (item) {
        const id = item.vendorCategoryId || item.VendorCategoryId;
        const name = item.name || item.Name || "";
        const budgetLabel =
          item.parentBudgetCodeLabel ||
          item.ParentBudgetCodeLabel ||
          "No Budget Code";
        const label = `${name} (${budgetLabel})`;
        coaTextOptionMap[String(id)] = label;
        $coa.append(`<option value="${id}">${label}</option>`);
      });

      $coa.prop("disabled", false);
      const availableIds = list.map((x) =>
        String(x.vendorCategoryId || x.VendorCategoryId),
      );
      const nextSelected = previousSelected.filter((x) =>
        availableIds.includes(String(x)),
      );
      $coa.val(nextSelected);
      initBudgetCoaSelect2();
    },
    error: function () {
      $coa.html("").prop("disabled", true).val([]).trigger("change");
      coaTextOptionMap = {};
      initBudgetCoaSelect2();
    },
  });
}

// Load COA Text options for Budget Summary filter (cascade from budget code)
function loadBudgetSummaryCOAOptions(budgetCodeId) {
  const $sel = $("#budgetSummaryCOAFilter");
  $sel.find("option:not([value=''])").remove();
  if (!budgetCodeId) {
    $sel.prop("disabled", true).val("");
    return;
  }
  $.ajax({
    url: `/api/vendor-categories/by-budget-codes?ids=${encodeURIComponent(budgetCodeId)}`,
    type: "GET",
    success: function (rows) {
      if (rows && rows.length) {
        rows.forEach(function (row) {
          $sel.append(
            $("<option>")
              .val(row.vendorCategoryId)
              .text(row.name || row.vendorCategoryId),
          );
        });
        $sel.prop("disabled", false);
      } else {
        $sel.prop("disabled", true);
      }
    },
    error: function () {
      $sel.prop("disabled", true);
    },
  });
}

// Load Budget Summary
function loadBudgetSummary(year) {
  const budgetCodeId = $("#budgetSummaryBudgetCodeFilter").val() || "";
  const vendorCategoryId = $("#budgetSummaryCOAFilter").val() || "";
  const query = new URLSearchParams();
  if (budgetCodeId) query.append("budgetCodeId", budgetCodeId);
  if (vendorCategoryId) query.append("vendorCategoryId", vendorCategoryId);
  const qs = query.toString() ? "?" + query.toString() : "";
  $.ajax({
    url: `/api/dashboard/budget-summary/${year}${qs}`,
    type: "GET",
    success: function (data) {
      // Update summary cards
      $("#totalBudget").text(
        "Rp " + Math.round(data.totalBudget).toLocaleString("id-ID"),
      );
      $("#totalRealisasi").text(
        "Rp " + Math.round(data.totalRealisasi).toLocaleString("id-ID"),
      );
      $("#sisaBudget").text(
        "Rp " + Math.round(data.totalSisaBudget).toLocaleString("id-ID"),
      );
      $("#persentaseSerapan").text(
        data.overallPersentaseSerapan.toFixed(2) + "%",
      );

      // Update progress bar
      $("#serapanProgressBar").css(
        "width",
        data.overallPersentaseSerapan + "%",
      );

      // Update traffic light
      const trafficLight = $("#trafficLight");
      trafficLight.removeClass("bg-success bg-warning bg-danger");

      if (data.overallTrafficLight === "green") {
        trafficLight.addClass("bg-success");
        $("#serapanProgressBar")
          .removeClass("bg-warning bg-danger")
          .addClass("bg-success");
      } else if (data.overallTrafficLight === "yellow") {
        trafficLight.addClass("bg-warning");
        $("#serapanProgressBar")
          .removeClass("bg-success bg-danger")
          .addClass("bg-warning");
      } else {
        trafficLight.addClass("bg-danger");
        $("#serapanProgressBar")
          .removeClass("bg-success bg-warning")
          .addClass("bg-danger");
      }
    },
    error: function (xhr) {
      console.error("Failed to load budget summary:", xhr);
    },
  });
}

function loadBudgetCoaChart(year) {
  if (!year) year = parseInt($("#budgetYearFilter").val());

  const startDate = $("#budgetCoaStartDate").val();
  const endDate = $("#budgetCoaEndDate").val();
  const budgetCodeIds = ($("#budgetCoaBudgetCodeFilter").val() || []).filter(
    Boolean,
  );
  const coaTextIds = ($("#budgetCoaTextFilter").val() || []).filter(Boolean);

  const query = new URLSearchParams();
  if (startDate) query.append("startDate", startDate);
  if (endDate) query.append("endDate", endDate);
  if (budgetCodeIds.length)
    query.append("budgetCodeIds", budgetCodeIds.join(","));
  if (coaTextIds.length) query.append("coaTextIds", coaTextIds.join(","));

  $.ajax({
    url: `/api/dashboard/budget-coa-performance/${year}?${query.toString()}`,
    type: "GET",
    success: function (data) {
      if (data && data.length > 0) {
        budgetCoaRows = data;
        // Y-axis: hanya CoaText (KodeAnggaran) — tanpa deskripsi untuk menghindari label terlalu panjang / double dash
        const coaTextNames = data.map((v) => {
          const codeOnly = v.budgetCode
            ? v.budgetCode.split(" - ")[0].trim()
            : "";
          return `${v.coaText}${codeOnly ? " (" + codeOnly + ")" : ""}`;
        });
        // Tooltip: label lengkap dari displayLabel
        const coaDisplayLabels = data.map(
          (v) =>
            v.displayLabel ||
            `${v.coaText} (${v.budgetCode || "No Budget Code"})`,
        );
        const coaTextNamesWrapped = coaTextNames.map((label) =>
          wrapLabelTwoLines(label, 28),
        );
        const plans = data.map((v) => Number(v.plan || 0));
        const actuals = data.map((v) => Number(v.actual || 0));
        const dynamicHeight = Math.max(420, data.length * 62);
        const maxAxisValue = Math.max(0, ...plans, ...actuals);
        const longestAxisLabel = formatRupiah(maxAxisValue).length;
        const chartHost = document.querySelector("#budget-coa-chart");
        const hostWidth = chartHost?.parentElement?.clientWidth || 0;
        const minReadableWidth = Math.max(
          700,
          Math.min(1600, 420 + longestAxisLabel * 18),
        );
        const chartWidth = Math.max(hostWidth, minReadableWidth);

        if (chartHost) {
          chartHost.style.overflowX = "auto";
          chartHost.style.overflowY = "hidden";
          chartHost.style.webkitOverflowScrolling = "touch";
        }

        const chartOptions = {
          series: [
            {
              name: "Plan",
              data: plans,
            },
            {
              name: "Actual",
              data: actuals,
            },
          ],
          chart: {
            type: "bar",
            height: dynamicHeight,
            width: chartWidth,
            offsetX: 10,
            toolbar: {
              show: false,
            },
          },
          colors: ["#3b76ef", "#10b981"],
          plotOptions: {
            bar: {
              horizontal: true,
              barHeight: "62%",
              borderRadius: 5,
            },
          },
          dataLabels: {
            enabled: false,
          },
          stroke: {
            show: true,
            width: 2,
            colors: ["transparent"],
          },
          xaxis: {
            categories: coaTextNamesWrapped,
            tickAmount: 4,
            title: {
              text: "Nilai (Rupiah)",
            },
            labels: {
              trim: false,
              hideOverlappingLabels: false,
              formatter: function (val) {
                return formatRupiah(val);
              },
            },
          },
          yaxis: {
            labels: {
              maxWidth: 280,
              trim: false,
              style: {
                fontSize: "11px",
              },
            },
          },
          fill: {
            opacity: 1,
          },
          tooltip: {
            theme: "light",
            shared: true,
            intersect: false,
            x: {
              formatter: function (val, opts) {
                if (opts && opts.dataPointIndex !== undefined) {
                  return coaDisplayLabels[opts.dataPointIndex] || val;
                }
                return val;
              },
            },
            y: {
              formatter: function (val) {
                return formatRupiah(val);
              },
            },
          },
          legend: {
            position: "top",
            horizontalAlign: "right",
          },
        };

        if (budgetCoaChart) {
          budgetCoaChart.destroy();
        }

        budgetCoaChart = new ApexCharts(
          document.querySelector("#budget-coa-chart"),
          chartOptions,
        );
        budgetCoaChart.render();
      } else {
        budgetCoaRows = [];
        $("#budget-coa-chart").html(
          '<div class="text-center py-5 text-muted">No data available for selected filter</div>',
        );
      }
    },
    error: function (xhr) {
      console.error("Failed to load COA performance chart:", xhr);
      budgetCoaRows = [];
      $("#budget-coa-chart").html(
        '<div class="text-center py-5 text-danger">Failed to load data</div>',
      );
    },
  });
}

function exportBudgetCoaAsPng() {
  if (!budgetCoaChart) {
    alert("Chart belum tersedia untuk di-export.");
    return;
  }

  budgetCoaChart.dataURI().then(({ imgURI }) => {
    const a = document.createElement("a");
    a.href = imgURI;
    a.download = `budget-coa-performance-${new Date().toISOString().slice(0, 10)}.png`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
  });
}

function exportBudgetCoaAsCsv() {
  if (!budgetCoaRows || budgetCoaRows.length === 0) {
    alert("Data chart kosong, tidak ada yang bisa di-export.");
    return;
  }

  const headers = ["COA Text", "Plan", "Actual", "Variance", "Utilization %"];
  const rows = budgetCoaRows.map((r) => [
    r.displayLabel ||
      `${r.coaText || ""} (${r.budgetCode || "No Budget Code"})`,
    Number(r.plan || 0),
    Number(r.actual || 0),
    Number(r.variance || 0),
    Number(r.utilizationPercentage || 0).toFixed(2),
  ]);

  const csv = [headers, ...rows]
    .map((cols) =>
      cols.map((c) => `"${String(c).replace(/"/g, '""')}"`).join(","),
    )
    .join("\n");

  const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = `budget-coa-performance-${new Date().toISOString().slice(0, 10)}.csv`;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}

// Load Monthly Realisasi Trend Chart
function loadMonthlyRealisasiChart(year) {
  if (!year) year = parseInt($("#budgetYearFilter").val());

  $.ajax({
    url: `/api/dashboard/monthly-realisasi/${year}`,
    type: "GET",
    success: function (data) {
      if (data && data.length > 0) {
        const months = data.map((m) => m.monthName);
        const realisasi = data.map((m) => m.realisasi);
        const budgets = data.map((m) => m.budget);

        const chartOptions = {
          series: [
            {
              name: "Realisasi",
              data: realisasi,
            },
            {
              name: "Budget per Bulan",
              data: budgets,
            },
          ],
          chart: {
            type: "line",
            height: 400,
            toolbar: {
              show: false,
            },
          },
          colors: ["#10b981", "#ef4444"],
          stroke: {
            width: [3, 2],
            curve: "smooth",
            dashArray: [0, 5],
          },
          markers: {
            size: 5,
            colors: ["#10b981", "#ef4444"],
            strokeColors: "#fff",
            strokeWidth: 2,
            hover: {
              size: 7,
            },
          },
          xaxis: {
            categories: months,
          },
          yaxis: {
            title: {
              text: "Amount (Rupiah)",
            },
            labels: {
              formatter: function (val) {
                return "Rp " + (val / 1000000).toFixed(0) + "M";
              },
            },
          },
          tooltip: {
            theme: "light",
            y: {
              formatter: function (val) {
                return "Rp " + val.toLocaleString("id-ID");
              },
            },
          },
          legend: {
            position: "top",
            horizontalAlign: "right",
          },
        };

        if (monthlyRealisasiChart) {
          monthlyRealisasiChart.destroy();
        }

        monthlyRealisasiChart = new ApexCharts(
          document.querySelector("#monthly-realisasi-chart"),
          chartOptions,
        );
        monthlyRealisasiChart.render();
      } else {
        $("#monthly-realisasi-chart").html(
          '<div class="text-center py-5 text-muted">No data available</div>',
        );
      }
    },
    error: function (xhr) {
      console.error("Failed to load monthly realisasi:", xhr);
      $("#monthly-realisasi-chart").html(
        '<div class="text-center py-5 text-danger">Failed to load data</div>',
      );
    },
  });
}

// ==============================
// Load Dashboard Statistics
// ==============================
function loadDashboardStats() {
  const token = localStorage.getItem("token");

  $.ajax({
    url: "/api/dashboard/stats",
    type: "GET",
    headers: {
      Authorization: "Bearer " + token,
    },
    success: function (data) {
      // Update Active Contracts
      $("#activeContractsCount").text(data.activeContractsCount);

      // Update Contracts Expiring Soon with warning if > 0
      $("#contractsExpiringSoon").text(data.contractsExpiringSoon);
      if (data.contractsExpiringSoon > 0) {
        $("#contractsExpiringSoon").addClass("text-warning");
      } else {
        $("#contractsExpiringSoon")
          .removeClass("text-warning")
          .addClass("text-success");
      }

      // Update Total Invoices
      $("#totalInvoicesSubmitted").text(data.totalInvoicesSubmitted);

      // Update Total Invoice Amount
      const formattedAmount =
        "Rp " + (data.totalInvoiceAmount / 1000000).toFixed(2) + "M";
      $("#totalInvoiceAmount").text(formattedAmount);
    },
    error: function (xhr) {
      console.error("Failed to load dashboard stats:", xhr);
      $("#activeContractsCount").text("0");
      $("#contractsExpiringSoon").text("0");
      $("#totalInvoicesSubmitted").text("0");
      $("#totalInvoiceAmount").text("Rp 0");
    },
  });
}

// ==============================
// Vendor On-Time Submission KPI
// ==============================
let ontimeSubmissionChart = null;
let vendorPerformanceData = [];
let vendorPerformanceCurrentPage = 1;
const vendorPerformancePerPage = 5;

function loadOnTimeSubmissionKpi() {
  // Get year from filter, or use current year as fallback
  const $yearFilter = $("#ontimeYearFilter");
  let year = null;

  if ($yearFilter.length > 0) {
    year = $yearFilter.val();
  } else {
    console.warn("Filter element not found, using default (all years)");
  }

  const url = year
    ? `/api/dashboard/vendor-ontime-submission?year=${year}`
    : "/api/dashboard/vendor-ontime-submission";

  // Check if table element exists
  const $tbody = $("#vendorPerformanceTable");
  if ($tbody.length === 0) {
    console.error(
      "ERROR: Table element #vendorPerformanceTable NOT FOUND in DOM!",
    );
    return;
  }

  $.ajax({
    url: url,
    type: "GET",
    success: function (data) {
      // Update overall stats
      $("#ontimeTotalInvoices").text(data.totalInvoices || 0);
      $("#ontimeOnTimeCount").text(data.onTimeSubmissions || 0);
      $("#ontimeLateCount").text(data.lateSubmissions || 0);
      $("#ontimePercentage").text(
        (data.onTimePercentage || 0).toFixed(2) + "%",
      );

      // Update progress bar
      const percentage = data.onTimePercentage || 0;
      $("#ontimeProgressBar").css("width", percentage + "%");

      // Set color and badge based on performance
      let progressBarClass = "bg-success";
      let badgeText = "Excellent";
      let badgeClass = "bg-success";

      if (percentage < 70) {
        progressBarClass = "bg-danger";
        badgeText = "Poor";
        badgeClass = "bg-danger";
      } else if (percentage < 90) {
        progressBarClass = "bg-warning";
        badgeText = "Good";
        badgeClass = "bg-warning";
      }

      $("#ontimeProgressBar")
        .removeClass("bg-success bg-warning bg-danger")
        .addClass(progressBarClass);
      $("#ontimeStatusBadge")
        .removeClass("bg-success bg-warning bg-danger")
        .addClass(badgeClass)
        .text(badgeText);

      // Render donut chart
      renderOnTimeChart(data);

      // Render vendor performance table
      renderVendorPerformanceTable(data.vendorBreakdown);
    },
    error: function (xhr, status, error) {
      console.error(
        "Failed to load on-time submission KPI:",
        xhr,
        status,
        error,
      );
      console.error("Response:", xhr.responseText);

      $("#ontimeTotalInvoices").text("0");
      $("#ontimeOnTimeCount").text("0");
      $("#ontimeLateCount").text("0");
      $("#ontimePercentage").text("0%");

      // Show error in table
      const $tbody = $("#vendorPerformanceTable");
      $tbody.empty();
      $tbody.append(
        `<tr><td colspan="5" class="text-center text-danger py-4">Failed to load data: ${error || "Unknown error"}</td></tr>`,
      );
      $("#vendorPerformancePagination").hide();
    },
  });
}

function renderOnTimeChart(data) {
  const chartOptions = {
    series: [data.onTimeSubmissions, data.lateSubmissions],
    chart: {
      type: "donut",
      height: 350,
    },
    labels: ["On-Time", "Late"],
    colors: ["#10b981", "#ef4444"],
    legend: {
      position: "bottom",
      fontSize: "14px",
    },
    plotOptions: {
      pie: {
        donut: {
          size: "65%",
          labels: {
            show: true,
            name: {
              show: true,
              fontSize: "18px",
              fontWeight: 600,
              offsetY: -10,
            },
            value: {
              show: true,
              fontSize: "24px",
              fontWeight: 700,
              offsetY: 5,
              formatter: function (val) {
                return val;
              },
            },
            total: {
              show: true,
              label: "Total Invoices",
              fontSize: "14px",
              fontWeight: 400,
              color: "#9ca3af",
              formatter: function (w) {
                return w.globals.seriesTotals.reduce((a, b) => {
                  return a + b;
                }, 0);
              },
            },
          },
        },
      },
    },
    dataLabels: {
      enabled: true,
      formatter: function (val, opts) {
        return opts.w.config.series[opts.seriesIndex];
      },
      style: {
        fontSize: "14px",
        fontWeight: 600,
      },
    },
    tooltip: {
      theme: "light",
      y: {
        formatter: function (val, opts) {
          const percentage = ((val / data.totalInvoices) * 100).toFixed(1);
          return val + " (" + percentage + "%)";
        },
      },
    },
  };

  if (ontimeSubmissionChart) {
    ontimeSubmissionChart.destroy();
  }

  ontimeSubmissionChart = new ApexCharts(
    document.querySelector("#ontime-submission-chart"),
    chartOptions,
  );
  ontimeSubmissionChart.render();
}

function renderVendorPerformanceTable(vendors) {
  vendorPerformanceData = Array.isArray(vendors) ? vendors : [];
  vendorPerformanceCurrentPage = 1;
  updateVendorPerformanceTable();
  updateVendorPerformancePagination();
}

function updateVendorPerformanceTable() {
  const $tbody = $("#vendorPerformanceTable");
  $tbody.empty();

  if (!vendorPerformanceData || vendorPerformanceData.length === 0) {
    $tbody.append(
      '<tr><td colspan="5" class="text-center text-muted py-4">No vendor data available</td></tr>',
    );
    return;
  }

  const startIndex =
    (vendorPerformanceCurrentPage - 1) * vendorPerformancePerPage;
  const endIndex = startIndex + vendorPerformancePerPage;
  const paginatedVendors = vendorPerformanceData.slice(startIndex, endIndex);

  paginatedVendors.forEach((vendor, localIndex) => {
    const index = startIndex + localIndex;
    const rank = index + 1;
    const percentage = vendor.onTimePercentage.toFixed(1);

    // Determine badge color based on performance
    let badgeClass = "bg-success";
    let badgeText = vendor.performanceStatus;

    if (vendor.performanceStatus === "Poor") {
      badgeClass = "bg-danger";
    } else if (vendor.performanceStatus === "Good") {
      badgeClass = "bg-warning";
    }

    // Add rank badge for top 3
    let rankBadge = "";
    if (index === 0) {
      rankBadge = '<i class="feather-award text-warning me-2"></i>';
    } else if (index === 1) {
      rankBadge = '<i class="feather-award text-muted me-2"></i>';
    } else if (index === 2) {
      rankBadge = '<i class="feather-award text-secondary me-2"></i>';
    }

    let rankStyle = "color:#64748b; font-weight:500;";
    if (rank === 1) {
      rankStyle = "color:#d4af37; font-weight:700;";
    } else if (rank === 2) {
      rankStyle = "color:#9ca3af; font-weight:700;";
    } else if (rank === 3) {
      rankStyle = "color:#cd7f32; font-weight:700;";
    }

    const row = `
            <tr>
                <td class="text-center">
                    <span style="${rankStyle}">${rank}</span>
                </td>
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

function updateVendorPerformancePagination() {
  const totalPages = Math.ceil(
    vendorPerformanceData.length / vendorPerformancePerPage,
  );
  const $pagination = $("#vendorPerformancePagination");

  if ($pagination.length === 0) {
    return;
  }

  if (!vendorPerformanceData || totalPages <= 1) {
    $pagination.hide();
    return;
  }

  $pagination.show();
  $pagination.empty();

  $pagination.append(`
        <li>
            <a href="javascript:void(0);" id="vendorPerfPrevPage" ${vendorPerformanceCurrentPage === 1 ? 'class="disabled"' : ""}>
                <i class="bi bi-arrow-left"></i>
            </a>
        </li>
    `);

  let startPage = Math.max(1, vendorPerformanceCurrentPage - 1);
  let endPage = Math.min(totalPages, startPage + 2);

  if (endPage - startPage < 2) {
    startPage = Math.max(1, endPage - 2);
  }

  if (startPage > 1) {
    $pagination.append(
      '<li><a href="javascript:void(0);" data-page="1">1</a></li>',
    );
    if (startPage > 2) {
      $pagination.append(
        '<li><a href="javascript:void(0);"><i class="bi bi-dot"></i></a></li>',
      );
    }
  }

  for (let i = startPage; i <= endPage; i++) {
    $pagination.append(`
            <li>
                <a href="javascript:void(0);" data-page="${i}" class="${i === vendorPerformanceCurrentPage ? "active" : ""}">${i}</a>
            </li>
        `);
  }

  if (endPage < totalPages) {
    if (endPage < totalPages - 1) {
      $pagination.append(
        '<li><a href="javascript:void(0);"><i class="bi bi-dot"></i></a></li>',
      );
    }
    $pagination.append(
      `<li><a href="javascript:void(0);" data-page="${totalPages}">${totalPages}</a></li>`,
    );
  }

  $pagination.append(`
        <li>
            <a href="javascript:void(0);" id="vendorPerfNextPage" ${vendorPerformanceCurrentPage === totalPages ? 'class="disabled"' : ""}>
                <i class="bi bi-arrow-right"></i>
            </a>
        </li>
    `);

  $pagination
    .find("a[data-page]")
    .off("click")
    .on("click", function (e) {
      e.preventDefault();
      const page = parseInt($(this).data("page"));
      if (page) {
        vendorPerformanceCurrentPage = page;
        updateVendorPerformanceTable();
        updateVendorPerformancePagination();
      }
    });

  $("#vendorPerfPrevPage")
    .off("click")
    .on("click", function (e) {
      e.preventDefault();
      if (vendorPerformanceCurrentPage > 1) {
        vendorPerformanceCurrentPage--;
        updateVendorPerformanceTable();
        updateVendorPerformancePagination();
      }
    });

  $("#vendorPerfNextPage")
    .off("click")
    .on("click", function (e) {
      e.preventDefault();
      if (vendorPerformanceCurrentPage < totalPages) {
        vendorPerformanceCurrentPage++;
        updateVendorPerformanceTable();
        updateVendorPerformancePagination();
      }
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
    method: "GET",
    success: function (response) {
      //console.log('Contracts response:', response);
      if (response.success) {
        contractsData = response.data || [];
        contractsCurrentPage = 1;
        updateContractsExpiringTable();
        updateContractsPagination();
        updateContractSummary();
        $("#expiringContractsCount").text(response.count);
      } else {
        console.error("Response success is false:", response);
        $("#contractsExpiringTableBody").html(
          '<tr><td colspan="4" class="text-center text-danger py-4">Failed to load contracts: ' +
            (response.message || "Unknown error") +
            "</td></tr>",
        );
      }
    },
    error: function (xhr, status, error) {
      console.error("AJAX error:", { xhr, status, error });
      console.error("Response text:", xhr.responseText);
      let errorMessage = "Failed to load contracts";
      if (xhr.status === 401) {
        errorMessage = "Unauthorized - Please login again";
      } else if (xhr.status === 500) {
        try {
          const errorData = JSON.parse(xhr.responseText);
          errorMessage = errorData.message || errorMessage;
        } catch (e) {
          errorMessage = xhr.responseText || errorMessage;
        }
      }
      $("#contractsExpiringTableBody").html(
        '<tr><td colspan="4" class="text-center text-danger py-4">' +
          errorMessage +
          "</td></tr>",
      );
    },
  });
}

function updateContractsExpiringTable() {
  //console.log('Updating table with contracts:', contractsData);
  const $tbody = $("#contractsExpiringTableBody");
  $tbody.empty();

  if (!contractsData || contractsData.length === 0) {
    $tbody.append(
      '<tr><td colspan="4" class="text-center text-muted py-4">No contracts expiring soon</td></tr>',
    );
    $("#expiringContractsCount").text("0");
    return;
  }

  // Calculate pagination
  const startIndex = (contractsCurrentPage - 1) * contractsPerPage;
  const endIndex = startIndex + contractsPerPage;
  const paginatedContracts = contractsData.slice(startIndex, endIndex);

  paginatedContracts.forEach((contract) => {
    // Determine alert badge
    let alertBadge = "";
    let statusClass = "";

    if (contract.alertLevel === "Critical") {
      alertBadge = '<span class="badge bg-danger">Critical</span>';
      statusClass = "text-danger fw-bold";
    } else if (contract.alertLevel === "Warning") {
      alertBadge = '<span class="badge bg-warning">Warning</span>';
      statusClass = "text-warning fw-bold";
    } else {
      alertBadge = '<span class="badge bg-success">Safe</span>';
      statusClass = "text-success";
    }

    // Format countdown
    const countdownHtml = formatContractCountdown(contract.daysRemaining);

    // Format dates
    const endDate = new Date(contract.endDate).toLocaleDateString("id-ID", {
      day: "numeric",
      month: "short",
      year: "numeric",
    });

    const row = `
            <tr>
                <td>
                    <div class="hstack gap-2">
                        <span class="wd-10 ht-10 ${contract.alertLevel === "Critical" ? "bg-danger" : contract.alertLevel === "Warning" ? "bg-warning" : "bg-success"} rounded-circle d-inline-block me-2 lh-base"></span>
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
  const $pagination = $("#contractsPagination");

  if (totalPages <= 1) {
    $pagination.hide();
    return;
  }

  $pagination.show();
  $pagination.empty();

  // Previous button
  $pagination.append(`
        <li>
            <a href="javascript:void(0);" id="contractsPrevPage" ${contractsCurrentPage === 1 ? 'class="disabled"' : ""}>
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
    $pagination.append(
      `<li><a href="javascript:void(0);" data-page="1">1</a></li>`,
    );
    if (startPage > 2) {
      $pagination.append(
        `<li><a href="javascript:void(0);"><i class="bi bi-dot"></i></a></li>`,
      );
    }
  }

  for (let i = startPage; i <= endPage; i++) {
    $pagination.append(`
            <li>
                <a href="javascript:void(0);" data-page="${i}" class="${i === contractsCurrentPage ? "active" : ""}">${i}</a>
            </li>
        `);
  }

  if (endPage < totalPages) {
    if (endPage < totalPages - 1) {
      $pagination.append(
        `<li><a href="javascript:void(0);"><i class="bi bi-dot"></i></a></li>`,
      );
    }
    $pagination.append(
      `<li><a href="javascript:void(0);" data-page="${totalPages}">${totalPages}</a></li>`,
    );
  }

  // Next button
  $pagination.append(`
        <li>
            <a href="javascript:void(0);" id="contractsNextPage" ${contractsCurrentPage === totalPages ? 'class="disabled"' : ""}>
                <i class="bi bi-arrow-right"></i>
            </a>
        </li>
    `);

  // Bind events
  $pagination
    .find("a[data-page]")
    .off("click")
    .on("click", function (e) {
      e.preventDefault();
      const page = parseInt($(this).data("page"));
      if (page) {
        contractsCurrentPage = page;
        updateContractsExpiringTable();
        updateContractsPagination();
      }
    });

  $("#contractsPrevPage")
    .off("click")
    .on("click", function (e) {
      e.preventDefault();
      if (contractsCurrentPage > 1) {
        contractsCurrentPage--;
        updateContractsExpiringTable();
        updateContractsPagination();
      }
    });

  $("#contractsNextPage")
    .off("click")
    .on("click", function (e) {
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
  const critical = contractsData.filter(
    (c) => c.alertLevel === "Critical",
  ).length;
  const warning = contractsData.filter(
    (c) => c.alertLevel === "Warning",
  ).length;
  const safe = contractsData.filter((c) => c.alertLevel === "Safe").length;

  $("#criticalContractsCount").text(critical);
  $("#warningContractsCount").text(warning);
  $("#safeContractsCount").text(safe);
}

// Handler klik summary contract
$(document).on(
  "click",
  ".col-md-3 .p-3.border.border-dashed.rounded",
  function () {
    var title = $(this).find(".fs-12").text().trim();
    var status = "";
    if (title.toLowerCase().includes("active")) status = "Active";
    else if (title.toLowerCase().includes("expiring")) status = "Expiring Soon";
    else if (title.toLowerCase().includes("total invoices"))
      return; // skip
    else if (title.toLowerCase().includes("total invoice amount"))
      return; // skip
    else return;
    showContractsByStatusModal(status);
  },
);

// Show modal dan load data contract sesuai status
function showContractsByStatusModal(status) {
  $("#contractStatusModalLabel").text("Contract List - " + status);
  $("#contractStatusModalContent").html(
    '<div class="text-center py-4"><div class="spinner-border"></div></div>',
  );
  portalModalToBody("contractStatusModal");
  showModal("contractStatusModal");
  $.get(
    "/api/dashboard/contracts-by-status?status=" + encodeURIComponent(status),
    function (data) {
      if (data && data.length > 0) {
        var html =
          '<div class="table-responsive"><table class="table table-bordered table-hover"><thead><tr>' +
          "<th>No</th><th>Contract Number</th><th>Vendor</th><th>Description</th><th>Start Date</th><th>End Date</th><th>Status</th></tr></thead><tbody>";
        data.forEach(function (c, i) {
          html +=
            "<tr>" +
            "<td>" +
            (i + 1) +
            "</td>" +
            "<td>" +
            (c.contractNumber || "-") +
            "</td>" +
            "<td>" +
            (c.vendorName || "-") +
            "</td>" +
            "<td>" +
            (c.contractDescription || "-") +
            "</td>" +
            "<td>" +
            (c.startDate ? c.startDate.split("T")[0] : "-") +
            "</td>" +
            "<td>" +
            (c.endDate ? c.endDate.split("T")[0] : "-") +
            "</td>" +
            "<td>" +
            (c.status || "-") +
            "</td>" +
            "</tr>";
        });
        html += "</tbody></table></div>";
        $("#contractStatusModalContent").html(html);
      } else {
        $("#contractStatusModalContent").html(
          '<div class="text-center text-muted py-4">No contract data available for this status.</div>',
        );
      }
    },
  );
}

// Ensure modal is always portaled to body and cleaned up on hide (like invoice modal)
$(document).ready(function () {
  $("#contractStatusModal").on("shown.bs.modal", function () {
    $(this).find(".modal-body").attr("tabindex", "-1").trigger("focus");
  });
  $("#contractStatusModal").on("hide.bs.modal", function () {
    if (document.activeElement) {
      document.activeElement.blur();
    }
  });
  $("#contractStatusModal").on("hidden.bs.modal", function () {
    // Clean up content if needed
    $("#contractStatusModalContent").empty();
  });
});
