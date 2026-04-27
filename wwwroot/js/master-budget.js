// ==============================
// Budget Management JavaScript
// ==============================

let budgetTable;
let isEditMode = false;

// Ensure modal is appended to body to prevent z-index issues
function portalModalToBody(modalId) {
  const el = document.getElementById(modalId);
  if (!el) return;
  if (el.parentElement !== document.body) {
    document.body.appendChild(el);
  }
}

// Helper: authenticated fetch wrapper
function authFetchJson(url) {
  return $.ajax({ url: url, type: "GET", dataType: "json" });
}

$(document).ready(function () {
  // Load Budget Codes for dropdown
  loadBudgetCodes();

  // Initialize DataTable
  initDataTable();

  // Initialize Select2 on dropdowns after a short delay to ensure DOM is ready
  setTimeout(function () {
    initializeSelect2();
  }, 100);

  // Event: Add Budget Button
  $("#btnAddBudget").on("click", function () {
    openAddModal();
  });

  // Event: Form Submit
  $("#budgetForm").on("submit", function (e) {
    e.preventDefault();
    saveBudget();
  });

  // Format currency inputs
  $("#totalBudget, #monthlyBudget").on("input", function () {
    formatCurrencyInput(this);
  });

  // Auto calculate monthly budget
  $("#totalBudget").on("blur", function () {
    autoCalculateMonthly();
  });

  // ======== Cascading Dropdown Logic ========

  // When Budget Code changes → load Vendor Categories filtered by ParentBudgetCodeId
  $("#budgetCodeSelect").on("change", function () {
    const budgetCodeId = $(this).val();
    const $typeBudget = $("#typeBudgetSelect");
    const $noCoa = $("#noCoa");

    // Reset dependent fields
    $typeBudget
      .html('<option value="">-- Loading... --</option>')
      .prop("disabled", true);
    $noCoa.val("");

    // Destroy and reinitialize Select2 for typeBudgetSelect
    if ($typeBudget.hasClass("select2-hidden-accessible")) {
      $typeBudget.select2("destroy");
    }

    if (!budgetCodeId) {
      $typeBudget
        .html(
          '<option value="">-- Pilih Budget Code terlebih dahulu --</option>',
        )
        .prop("disabled", true);

      // Reinitialize Select2
      $typeBudget.select2({
        theme: "bootstrap-5",
        placeholder: "-- Search or Select COA Text --",
        allowClear: true,
        width: "100%",
        dropdownParent: $("#budgetModal"),
      });
      return;
    }

    // Fetch vendor categories that have ParentBudgetCodeId == selected BudgetCodeId
    authFetchJson(`/api/vendor-categories/by-budget-code/${budgetCodeId}`)
      .done(function (data) {
        const items = Array.isArray(data) ? data : data.items || [];
        let html = '<option value="">-- Select COA Text --</option>';
        items.forEach(function (vc) {
          html += `<option value="${vc.vendorCategoryId}" data-nocoa="${vc.noCoa || ""}" data-name="${vc.name || ""}">${vc.name}</option>`;
        });
        $typeBudget.html(html).prop("disabled", false);

        // Reinitialize Select2 after updating options
        $typeBudget.select2({
          theme: "bootstrap-5",
          placeholder: "-- Search or Select COA Text --",
          allowClear: true,
          width: "100%",
          dropdownParent: $("#budgetModal"),
        });
      })
      .fail(function () {
        $typeBudget
          .html('<option value="">-- Gagal memuat data --</option>')
          .prop("disabled", true);

        // Reinitialize Select2 even on failure
        $typeBudget.select2({
          theme: "bootstrap-5",
          placeholder: "-- Search or Select COA Text --",
          allowClear: true,
          width: "100%",
          dropdownParent: $("#budgetModal"),
        });
      });
  });

  // When COA Text (Vendor Category) changes → auto-fill NoCoa
  $("#typeBudgetSelect").on("change", function () {
    const $selected = $(this).find(":selected");
    const noCoa = $selected.data("nocoa") || "";
    $("#noCoa").val(noCoa);
  });
});

// Initialize Select2 on dropdowns for search functionality
function initializeSelect2() {
  // Initialize Budget Code dropdown with Select2
  $("#budgetCodeSelect").select2({
    theme: "bootstrap-5",
    placeholder: "-- Search or Select Budget Code --",
    allowClear: true,
    width: "100%",
    dropdownParent: $("#budgetModal"),
  });

  // Initialize COA Text (Vendor Category) dropdown with Select2
  $("#typeBudgetSelect").select2({
    theme: "bootstrap-5",
    placeholder: "-- Search or Select COA Text --",
    allowClear: true,
    width: "100%",
    dropdownParent: $("#budgetModal"),
  });
}

// Load all Budget Codes into the dropdown
function loadBudgetCodes() {
  authFetchJson("/api/budget-codes?page=1&pageSize=2000")
    .done(function (json) {
      const items = Array.isArray(json) ? json : json.items || [];
      let html = '<option value="">-- Select Budget Code --</option>';
      items.forEach(function (bc) {
        html += `<option value="${bc.budgetCodeId}">${bc.code} - ${bc.description}</option>`;
      });
      $("#budgetCodeSelect").html(html);

      // Reinitialize Select2 after updating options
      if ($("#budgetCodeSelect").hasClass("select2-hidden-accessible")) {
        $("#budgetCodeSelect").select2("destroy");
      }
      initializeSelect2();
    })
    .fail(function () {
      console.error("Failed to load budget codes");
    });
}

// Initialize DataTable
function initDataTable() {
  budgetTable = $("#budgetTable").DataTable({
    processing: true,
    serverSide: true,
    ajax: {
      url: "/api/budgets",
      type: "GET",
      data: function (d) {
        // DataTables will send: start, length, search[value], order, etc.
        // You can customize here if backend expects different params
        return d;
      },
      dataSrc: function (json) {
        // DataTables expects { data, recordsTotal, recordsFiltered, draw }
        return json.data || [];
      },
      error: function (xhr, error, thrown) {
        console.error("DataTable error:", error, thrown);
        showAlert(
          "Error loading data: " +
            (xhr.responseJSON?.message || "Unknown error"),
          "danger",
        );
      },
    },
    columns: [
      { data: "year", className: "fw-bold" },
      {
        data: "budgetCodeLabel",
        render: function (data) {
          return data || '<span class="text-muted">-</span>';
        },
      },
      {
        data: "noCoa",
        render: function (data) {
          return data || '<span class="text-muted">-</span>';
        },
      },
      {
        data: "typeBudget",
        render: function (data) {
          return data || '<span class="text-muted">-</span>';
        },
      },
      {
        data: "activity",
        render: function (data) {
          if (!data) return '<span class="text-muted">-</span>';
          return data.length > 50 ? data.substring(0, 50) + "..." : data;
        },
      },
      {
        data: "totalBudget",
        render: function (data) {
          return formatRupiah(data);
        },
      },
      {
        data: "monthlyBudget",
        render: function (data) {
          return formatRupiah(data);
        },
      },
      {
        data: "createdAt",
        render: function (data) {
          return data ? new Date(data).toLocaleDateString("id-ID") : "-";
        },
      },
      {
        data: null,
        className: "text-end dt-actions",
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
        },
      },
    ],
    order: [[0, "desc"]],
    pageLength: 10,
    scrollX: true,
    language: {
      emptyTable: "No budget data available",
      zeroRecords: "No matching budgets found",
    },
  });
}

// Open Add Modal
function openAddModal() {
  isEditMode = false;
  $("#budgetModalLabel").text("Add Budget");
  $("#budgetForm")[0].reset();
  $("#budgetId").val("");
  $("#budgetCodeSelect").val("").trigger("change");
  $("#typeBudgetSelect")
    .html('<option value="">-- Pilih Budget Code terlebih dahulu --</option>')
    .prop("disabled", true);

  // Reinitialize Select2 for clean state
  if ($("#typeBudgetSelect").hasClass("select2-hidden-accessible")) {
    $("#typeBudgetSelect").select2("destroy");
  }
  $("#typeBudgetSelect").select2({
    theme: "bootstrap-5",
    placeholder: "-- Search or Select COA Text --",
    allowClear: true,
    width: "100%",
    dropdownParent: $("#budgetModal"),
  });

  $("#noCoa").val("");
  $("#activity").val("");
  portalModalToBody("budgetModal");
  const modal = new bootstrap.Modal(document.getElementById("budgetModal"));
  modal.show();
}

// Edit Budget
function editBudget(budgetId) {
  isEditMode = true;
  $("#budgetModalLabel").text("Edit Budget");

  $.ajax({
    url: `/api/budgets/${budgetId}`,
    type: "GET",
    success: function (data) {
      $("#budgetId").val(data.budgetId);
      $("#year").val(data.year);
      $("#totalBudget").val(formatNumberForInput(data.totalBudget));
      $("#monthlyBudget").val(formatNumberForInput(data.monthlyBudget));
      $("#noCoa").val(data.noCoa || "");
      $("#activity").val(data.activity || "");

      // Set Budget Code dropdown and trigger cascade
      if (data.budgetCodeId) {
        $("#budgetCodeSelect").val(data.budgetCodeId).trigger("change");

        // Load vendor categories for this budget code, then set the selected one
        authFetchJson(
          `/api/vendor-categories/by-budget-code/${data.budgetCodeId}`,
        ).done(function (vcData) {
          const items = Array.isArray(vcData) ? vcData : vcData.items || [];
          let html = '<option value="">-- Select COA Text --</option>';
          items.forEach(function (vc) {
            html += `<option value="${vc.vendorCategoryId}" data-nocoa="${vc.noCoa || ""}" data-name="${vc.name || ""}">${vc.name}</option>`;
          });
          $("#typeBudgetSelect").html(html).prop("disabled", false);

          // Reinitialize Select2 after updating options
          if ($("#typeBudgetSelect").hasClass("select2-hidden-accessible")) {
            $("#typeBudgetSelect").select2("destroy");
          }
          $("#typeBudgetSelect").select2({
            theme: "bootstrap-5",
            placeholder: "-- Search or Select COA Text --",
            allowClear: true,
            width: "100%",
            dropdownParent: $("#budgetModal"),
          });

          // Try to match by typeBudget (Name)
          const matchOption = $("#typeBudgetSelect option").filter(function () {
            return $(this).data("name") === data.typeBudget;
          });
          if (matchOption.length > 0) {
            $("#typeBudgetSelect").val(matchOption.val()).trigger("change");
          }
        });
      } else {
        $("#budgetCodeSelect").val("").trigger("change");
        $("#typeBudgetSelect")
          .html(
            '<option value="">-- Pilih Budget Code terlebih dahulu --</option>',
          )
          .prop("disabled", true);

        // Reinitialize Select2
        if ($("#typeBudgetSelect").hasClass("select2-hidden-accessible")) {
          $("#typeBudgetSelect").select2("destroy");
        }
        $("#typeBudgetSelect").select2({
          theme: "bootstrap-5",
          placeholder: "-- Search or Select COA Text --",
          allowClear: true,
          width: "100%",
          dropdownParent: $("#budgetModal"),
        });
      }

      portalModalToBody("budgetModal");
      const modal = new bootstrap.Modal(document.getElementById("budgetModal"));
      modal.show();
    },
    error: function (xhr) {
      showAlert(
        "Failed to load budget data: " +
          (xhr.responseJSON?.message || "Unknown error"),
        "danger",
      );
    },
  });
}

// Save Budget (Create or Update)
function saveBudget() {
  const budgetId = $("#budgetId").val();

  // Get the selected vendor category name as typeBudget
  const typeBudgetName =
    $("#typeBudgetSelect option:selected").data("name") ||
    $("#typeBudgetSelect option:selected").text() ||
    "";

  const data = {
    budgetId: budgetId || "00000000-0000-0000-0000-000000000000",
    year: parseInt($("#year").val()),
    budgetCodeId: $("#budgetCodeSelect").val() || null,
    noCoa: $("#noCoa").val() || "",
    typeBudget: typeBudgetName.trim(),
    activity: $("#activity").val() || "",
    totalBudget: parseFloat(
      $("#totalBudget").val().replace(/\./g, "").replace(",", "."),
    ),
    monthlyBudget: parseFloat(
      $("#monthlyBudget").val().replace(/\./g, "").replace(",", "."),
    ),
    createdBy: "00000000-0000-0000-0000-000000000000",
  };

  const url = isEditMode ? `/api/budgets/${budgetId}` : "/api/budgets";
  const method = isEditMode ? "PUT" : "POST";

  $.ajax({
    url: url,
    type: method,
    contentType: "application/json",
    data: JSON.stringify(data),
    success: function (response) {
      const modalEl = document.getElementById("budgetModal");
      const modal = bootstrap.Modal.getInstance(modalEl);
      if (modal) modal.hide();
      budgetTable.ajax.reload();
      showAlert(
        isEditMode
          ? "Budget updated successfully!"
          : "Budget created successfully!",
        "success",
      );
    },
    error: function (xhr) {
      const errorMsg =
        xhr.responseJSON?.message ||
        xhr.responseJSON?.title ||
        "Failed to save budget";
      showAlert(errorMsg, "danger");
    },
  });
}

// Delete Budget
function deleteBudget(budgetId, year) {
  if (confirm(`Are you sure you want to delete budget for year ${year}?`)) {
    $.ajax({
      url: `/api/budgets/${budgetId}`,
      type: "DELETE",
      success: function () {
        budgetTable.ajax.reload();
        showAlert("Budget deleted successfully!", "success");
      },
      error: function (xhr) {
        showAlert(
          "Failed to delete budget: " +
            (xhr.responseJSON?.message || "Unknown error"),
          "danger",
        );
      },
    });
  }
}

// Auto Calculate Monthly Budget
function autoCalculateMonthly() {
  const totalBudget = parseFloat(
    $("#totalBudget").val().replace(/\./g, "").replace(",", "."),
  );
  if (!isNaN(totalBudget) && totalBudget > 0) {
    const monthlyBudget = totalBudget / 12;
    $("#monthlyBudget").val(formatNumberForInput(monthlyBudget));
  }
}

// Format Currency Input (Rupiah style)
function formatCurrencyInput(input) {
  let value = input.value.replace(/\D/g, "");
  if (value) {
    value = parseInt(value).toLocaleString("id-ID");
  }
  input.value = value;
}

// Format Number for Input
function formatNumberForInput(number) {
  if (!number) return "";
  return Math.round(number).toLocaleString("id-ID");
}

// Format Rupiah for Display
function formatRupiah(amount) {
  if (!amount && amount !== 0) return "Rp 0";
  return "Rp " + Math.round(amount).toLocaleString("id-ID");
}

// Show Alert
function showAlert(message, type = "info") {
  const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;

  // Remove existing alerts
  $(".main-content .alert").remove();

  // Add new alert at the top of main content
  $(".main-content").prepend(alertHtml);

  // Auto dismiss after 5 seconds
  setTimeout(function () {
    $(".main-content .alert").fadeOut("slow", function () {
      $(this).remove();
    });
  }, 5000);
}
