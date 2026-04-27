  const apis = {
    approval: "/api/approval-statuses",
    attachment: "/api/attachment-types",
    contract: "/api/contract-statuses",
    invoiceProgress: "/api/invoice-progress-statuses",
    vendorCategory: "/api/vendor-categories",
    budgetCode: "/api/budget-codes",
    advancedStatus: "/api/advanced-statuses",
    biayaRealisasiStatus: "/api/biaya-realisasi-statuses",
  };
  let advancedStatusTable;
  let biayaRealisasiStatusTable;
  function wireAdvancedStatus() {
    advancedStatusTable = $("#advancedStatusesTable").DataTable({
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
        url: `${apis.advancedStatus}?page=1&pageSize=2000`,
        dataSrc: function (json) { return normalizeToArray(json); },
      },
      columns: [
        { data: "code" },
        { data: "name" },
        {
          data: null,
          orderable: false,
          searchable: false,
          render: function (data, type, row) {
            return `
              <div class="hstack gap-1 justify-content-center flex-nowrap">
                <button type="button" class="btn btn-sm btn-light-brand btn-advancedstatus-edit" data-id="${row.advancedStatusId}">
                  <i class="feather-edit-2 me-1"></i> Edit
                </button>
                <button type="button" class="btn btn-sm btn-light-danger btn-advancedstatus-delete" data-id="${row.advancedStatusId}">
                  <i class="feather-trash-2 me-1"></i> Delete
                </button>
              </div>
            `;
          },
        },
      ],
      pageLength: 10,
      lengthMenu: [10, 20, 50, 100, 200],
      pagingType: "simple_numbers",
      scrollX: false,
      autoWidth: false,
      language: {
        search: "",
        searchPlaceholder: "Search advanced statuses... ",
        lengthMenu: "_MENU_ / page",
        info: "Showing _START_ to _END_ of _TOTAL_ advanced statuses",
        infoEmpty: "No items found",
        zeroRecords: "No matching items",
        paginate: {
          previous: '<i class="feather-chevron-left"></i>',
          next: '<i class="feather-chevron-right"></i>',
        },
      },
      columnDefs: [
        { targets: -1, className: "text-center text-nowrap dt-actions" },
      ],
    });

    $("#btnCreateAdvancedStatus").on("click", function () {
      $("#advancedStatusId").val("");
      $("#advancedStatusCode").val("");
      $("#advancedStatusName").val("");
      $("#advancedStatusModalLabel").text("Create Advanced Status");
      showModal("advancedStatusModal");
    });

    $("#advancedStatusesTable").on("click", ".btn-advancedstatus-edit", function () {
      const id = $(this).data("id");
      authFetch(`${apis.advancedStatus}/${id}`)
        .then((r) => (r.ok ? r.json() : Promise.reject(r)))
        .then((data) => {
          $("#advancedStatusModalLabel").text("Edit Advanced Status");
          $("#advancedStatusId").val(data.advancedStatusId);
          $("#advancedStatusCode").val(data.code);
          $("#advancedStatusName").val(data.name);
          showModal("advancedStatusModal");
        })
        .catch(() => swalError("Error", { message: "Unable to load advanced status." }));
    });

    $("#advancedStatusesTable").on("click", ".btn-advancedstatus-delete", function () {
      const id = $(this).data("id");
      confirmDelete("advanced status").then((result) => {
        if (!result.isConfirmed) return;
        authFetch(`${apis.advancedStatus}/${id}`, { method: "DELETE" })
          .then((r) => {
            if (r.status === 204 || r.ok) return;
            return safeRejectJson(r);
          })
          .then(() => {
            advancedStatusTable.ajax.reload(null, false);
            Swal.fire("Deleted", "Advanced status deleted.", "success");
          })
          .catch((err) => swalError("Error", err));
      });
    });

    $("#advancedStatusForm").on("submit", function (e) {
      e.preventDefault();
      const id = $("#advancedStatusId").val();
      const code = $("#advancedStatusCode").val();
      const name = $("#advancedStatusName").val();
      if (!code || !name) {
        swalError("Validation", { message: "Code dan Name wajib diisi." });
        return;
      }
      const payload = {
        advancedStatusId: id ? Number(id) : null,
        code,
        name,
      };
      const method = id ? "PUT" : "POST";
      const url = id ? `${apis.advancedStatus}/${id}` : apis.advancedStatus;
      // Pastikan payload benar-benar { dto: {...} }
      authFetch(url, {
        method,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      })
        .then((r) => {
          if (r.status === 201 || r.status === 204 || r.ok)
            return r.json().catch(() => ({}));
          return safeRejectJson(r);
        })
        .then(() => {
          hideModal("advancedStatusModal");
          advancedStatusTable.ajax.reload(null, false);
          Swal.fire("Saved", "Advanced status saved.", "success");
        })
        .catch((err) => swalError("Error", err));
    });
  }

  function wireBiayaRealisasiStatus() {
    biayaRealisasiStatusTable = $("#biayaRealisasiStatusesTable").DataTable({
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
        url: `${apis.biayaRealisasiStatus}?page=1&pageSize=2000`,
        dataSrc: function (json) { return normalizeToArray(json); },
      },
      columns: [
        { data: "code" },
        { data: "name" },
        {
          data: null,
          orderable: false,
          searchable: false,
          render: function (data, type, row) {
            return `
              <div class="hstack gap-1 justify-content-center flex-nowrap">
                <button type="button" class="btn btn-sm btn-light-brand btn-biayarealisasistatus-edit" data-id="${row.biayaRealisasiStatusId}">
                  <i class="feather-edit-2 me-1"></i> Edit
                </button>
                <button type="button" class="btn btn-sm btn-light-danger btn-biayarealisasistatus-delete" data-id="${row.biayaRealisasiStatusId}">
                  <i class="feather-trash-2 me-1"></i> Delete
                </button>
              </div>
            `;
          },
        },
      ],
      pageLength: 10,
      lengthMenu: [10, 20, 50, 100, 200],
      pagingType: "simple_numbers",
      scrollX: false,
      autoWidth: false,
      language: {
        search: "",
        searchPlaceholder: "Search biaya realisasi statuses... ",
        lengthMenu: "_MENU_ / page",
        info: "Showing _START_ to _END_ of _TOTAL_ biaya realisasi statuses",
        infoEmpty: "No items found",
        zeroRecords: "No matching items",
        paginate: {
          previous: '<i class="feather-chevron-left"></i>',
          next: '<i class="feather-chevron-right"></i>',
        },
      },
      columnDefs: [
        { targets: -1, className: "text-center text-nowrap dt-actions" },
      ],
    });

    $("#btnCreateBiayaRealisasiStatus").on("click", function () {
      $("#biayaRealisasiStatusId").val("");
      $("#biayaRealisasiStatusCode").val("");
      $("#biayaRealisasiStatusName").val("");
      $("#biayaRealisasiStatusModalLabel").text("Create Biaya Realisasi Status");
      showModal("biayaRealisasiStatusModal");
    });

    $("#biayaRealisasiStatusesTable").on("click", ".btn-biayarealisasistatus-edit", function () {
      const id = $(this).data("id");
      authFetch(`${apis.biayaRealisasiStatus}/${id}`)
        .then((r) => (r.ok ? r.json() : Promise.reject(r)))
        .then((data) => {
          $("#biayaRealisasiStatusModalLabel").text("Edit Biaya Realisasi Status");
          $("#biayaRealisasiStatusId").val(data.biayaRealisasiStatusId);
          $("#biayaRealisasiStatusCode").val(data.code);
          $("#biayaRealisasiStatusName").val(data.name);
          showModal("biayaRealisasiStatusModal");
        })
        .catch(() => swalError("Error", { message: "Unable to load biaya realisasi status." }));
    });

    $("#biayaRealisasiStatusesTable").on("click", ".btn-biayarealisasistatus-delete", function () {
      const id = $(this).data("id");
      confirmDelete("biaya realisasi status").then((result) => {
        if (!result.isConfirmed) return;
        authFetch(`${apis.biayaRealisasiStatus}/${id}`, { method: "DELETE" })
          .then((r) => {
            if (r.status === 204 || r.ok) return;
            return safeRejectJson(r);
          })
          .then(() => {
            biayaRealisasiStatusTable.ajax.reload(null, false);
            Swal.fire("Deleted", "Biaya realisasi status deleted.", "success");
          })
          .catch((err) => swalError("Error", err));
      });
    });

    $("#biayaRealisasiStatusForm").on("submit", function (e) {
      e.preventDefault();
      const id = $("#biayaRealisasiStatusId").val();
      const code = $("#biayaRealisasiStatusCode").val();
      const name = $("#biayaRealisasiStatusName").val();
      if (!code || !name) {
        swalError("Validation", { message: "Code dan Name wajib diisi." });
        return;
      }
      const payload = {
        biayaRealisasiStatusId: id ? Number(id) : null,
        code,
        name,
      };
      const method = id ? "PUT" : "POST";
      const url = id ? `${apis.biayaRealisasiStatus}/${id}` : apis.biayaRealisasiStatus;
      // Pastikan payload benar-benar { dto: {...} }
      authFetch(url, {
        method,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      })
        .then((r) => {
          if (r.status === 201 || r.status === 204 || r.ok)
            return r.json().catch(() => ({}));
          return safeRejectJson(r);
        })
        .then(() => {
          hideModal("biayaRealisasiStatusModal");
          biayaRealisasiStatusTable.ajax.reload(null, false);
          Swal.fire("Saved", "Biaya realisasi status saved.", "success");
        })
        .catch((err) => swalError("Error", err));
    });
  }
/* master-index.js
   One-page CRUD for Master data (except Vendor)
   Uses API endpoints:
   - /api/approval-statuses
   - /api/attachment-types
   - /api/contract-statuses
   - /api/invoice-progress-statuses
   - /api/vendor-categories
*/


(function ($) {
  "use strict";

  // --- Helper Functions (Global Scope) ---
  window.normalizeToArray = function (json) {
    if (!json) return [];
    if (Array.isArray(json)) return json;
    if (Array.isArray(json.items)) return json.items;
    if (Array.isArray(json.data)) return json.data;
    return [];
  };

  window.portalModalToBody = function (modalId) {
    const el = document.getElementById(modalId);
    if (!el) return;
    if (el.parentElement !== document.body) document.body.appendChild(el);
  };

  window.showModal = function (modalId) {
    const el = document.getElementById(modalId);
    if (!el) return;
    const modal = new bootstrap.Modal(el);
    modal.show();
  };

  window.hideModal = function (modalId) {
    const el = document.getElementById(modalId);
    if (!el) return;
    const modal = bootstrap.Modal.getInstance(el);
    if (modal) modal.hide();
  };

  window.swalError = function (title, err) {
    const message =
      (err && (err.message || err.title)) ||
      (err && err.errors && Array.isArray(err.errors)
        ? err.errors.join(", ")
        : null) ||
      "Something went wrong.";
    Swal.fire(title || "Error", message, "error");
  };

  window.safeRejectJson = function (r) {
    return r.text().then((text) => {
      let b;
      try {
        b = JSON.parse(text);
      } catch {
        b = { message: `Request failed (${r.status})` };
      }
      return Promise.reject(b);
    });
  };

  window.confirmDelete = function (label) {
    return Swal.fire({
      title: `Delete ${label}?`,
      text: "This action cannot be undone.",
      icon: "warning",
      showCancelButton: true,
      confirmButtonText: "Delete",
      cancelButtonText: "Cancel",
    });
  };

  function pad2(n) {
    return String(n).padStart(2, "0");
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
    const blob =
      content instanceof Blob
        ? content
        : new Blob([content], { type: mimeType });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    a.remove();
    URL.revokeObjectURL(url);
  }

  function csvEscape(value) {
    if (value === null || value === undefined) return "";
    const s = String(value);
    const needsQuote = /[\r\n",]/.test(s);
    const escaped = s.replace(/"/g, '""');
    return needsQuote ? `"${escaped}"` : escaped;
  }

  function htmlEscape(value) {
    if (value === null || value === undefined) return "";
    return String(value)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#39;");
  }

  function getFilteredRows(dataTable) {
    if (!dataTable) return [];
    try {
      return dataTable.rows({ search: "applied" }).data().toArray();
    } catch {
      return [];
    }
  }

  function exportToCsv(dataTable, columns, filenameBase) {
    const rows = getFilteredRows(dataTable);
    const headers = columns.map((c) => csvEscape(c.header)).join(",");
    const lines = rows.map((r) =>
      columns.map((c) => csvEscape(c.value(r))).join(","),
    );
    // Add UTF-8 BOM so Excel opens UTF-8 nicely
    const csv = "\ufeff" + [headers, ...lines].join("\r\n");
    const filename = `${filenameBase}_${buildTimestampForFilename()}.csv`;
    downloadBlob(filename, "text/csv;charset=utf-8", csv);
  }

  function exportToExcelHtml(dataTable, columns, filenameBase) {
    const rows = getFilteredRows(dataTable);
    const head = `<tr>${columns.map((c) => `<th>${htmlEscape(c.header)}</th>`).join("")}</tr>`;
    const body = rows
      .map(
        (r) =>
          `<tr>${columns.map((c) => `<td>${htmlEscape(c.value(r))}</td>`).join("")}</tr>`,
      )
      .join("");
    const html = `<!doctype html><html><head><meta charset="utf-8"></head><body><table>${head}${body}</table></body></html>`;
    const filename = `${filenameBase}_${buildTimestampForFilename()}.xls`;
    downloadBlob(filename, "application/vnd.ms-excel;charset=utf-8", html);
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
      serverSide: true,
      processing: true,
      ajax: {
        url: apiBase,
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
          Swal.fire("Error", "Error loading data: " + (xhr.responseJSON?.message || "Unknown error"), "error");
        },
      },
      columns,
      pageLength: 10,
      lengthMenu: [10, 20, 50, 100, 200],
      pagingType: "simple_numbers",
      scrollX: true,
      autoWidth: false,
      language: {
        search: "",
        searchPlaceholder:
          (options && options.searchPlaceholder) || "Search... ",
        lengthMenu: "_MENU_ / page",
        info:
          (options && options.infoText) ||
          "Showing _START_ to _END_ of _TOTAL_ items",
        infoEmpty: "No items found",
        zeroRecords: "No matching items",
        paginate: {
          previous: '<i class="feather-chevron-left"></i>',
          next: '<i class="feather-chevron-right"></i>',
        },
      },
      columnDefs: [
        { targets: "_all", className: "text-nowrap" },
        {
          targets: -1,
          className: "text-center text-nowrap",
          width: 170,
          createdCell: function (td) {
            td.classList.add("dt-actions");
          },
        },
      ],
    });

    return table;
  }

  const apis = {
    approval: "/api/approval-statuses",
    attachment: "/api/attachment-types",
    contract: "/api/contract-statuses",
    invoiceProgress: "/api/invoice-progress-statuses",
    vendorCategory: "/api/vendor-categories",
    budgetCode: "/api/budget-codes",
  };

  let approvalTable;
  let attachmentTable;
  let contractTable;
  let invoiceProgressTable;
  let vendorCategoryTable;
  let budgetCodeTable;

  function wireApproval() {
    approvalTable = initSimpleTable(
      "#approvalStatusesTable",
      apis.approval,
      [
        { data: "code" },
        { data: "name" },
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
          },
        },
      ],
      {
        searchPlaceholder: "Search approval statuses... ",
        infoText: "Showing _START_ to _END_ of _TOTAL_ approval statuses",
      },
    );

    $("#btnCreateApprovalStatus").on("click", function () {
      $("#approvalStatusId").val("");
      $("#approvalCode").val("");
      $("#approvalName").val("");
      $("#approvalModalLabel").text("Create Approval Status");
      showModal("approvalModal");
    });

    $("#approvalStatusesTable").on("click", ".btn-approval-edit", function () {
      const id = $(this).data("id");
      authFetch(`${apis.approval}/${id}`)
        .then((r) => (r.ok ? r.json() : Promise.reject(r)))
        .then((data) => {
          $("#approvalModalLabel").text("Edit Approval Status");
          $("#approvalStatusId").val(data.approvalStatusId);
          $("#approvalCode").val(data.code);
          $("#approvalName").val(data.name);
          showModal("approvalModal");
        })
        .catch(() =>
          swalError("Error", { message: "Unable to load approval status." }),
        );
    });

    $("#approvalStatusesTable").on(
      "click",
      ".btn-approval-delete",
      function () {
        const id = $(this).data("id");
        confirmDelete("approval status").then((result) => {
          if (!result.isConfirmed) return;
          authFetch(`${apis.approval}/${id}`, { method: "DELETE" })
            .then((r) => {
              if (r.status === 204 || r.ok) return;
              return safeRejectJson(r);
            })
            .then(() => {
              approvalTable.ajax.reload(null, false);
              Swal.fire("Deleted", "Approval status deleted.", "success");
            })
            .catch((err) => swalError("Error", err));
        });
      },
    );

    $("#approvalForm").on("submit", function (e) {
      e.preventDefault();
      const id = $("#approvalStatusId").val();
      const payload = {
        approvalStatusId: id ? Number(id) : undefined,
        code: $("#approvalCode").val(),
        name: $("#approvalName").val(),
      };
      const method = id ? "PUT" : "POST";
      const url = id ? `${apis.approval}/${id}` : apis.approval;
      authFetch(url, {
        method,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      })
        .then((r) => {
          if (r.status === 201 || r.status === 204 || r.ok)
            return r.json().catch(() => ({}));
          return safeRejectJson(r);
        })
        .then(() => {
          hideModal("approvalModal");
          approvalTable.ajax.reload(null, false);
          Swal.fire("Saved", "Approval status saved.", "success");
        })
        .catch((err) => swalError("Error", err));
    });

    const cols = [
      { header: "Code", value: (r) => r.code },
      { header: "Name", value: (r) => r.name },
    ];

    $("#btnExportApprovalCsv").on("click", function (e) {
      e.preventDefault();
      exportToCsv(approvalTable, cols, "approval_status");
    });
    $("#btnExportApprovalExcel").on("click", function (e) {
      e.preventDefault();
      exportToExcelHtml(approvalTable, cols, "approval_status");
    });
  }

  function wireAttachment() {
    // Function to update label based on checkbox state
    function updateAttachmentIsRequiredLabel() {
      const isChecked = $("#attachmentIsRequired").is(":checked");
      const label = $("#attachmentIsRequiredLabel");
      if (isChecked) {
        label.html('<span class="badge bg-success">Wajib</span>');
      } else {
        label.html('<span class="badge bg-secondary">Tidak Wajib</span>');
      }
    }

    // Listen to checkbox change event
    $(document).on("change", "#attachmentIsRequired", function () {
      updateAttachmentIsRequiredLabel();
    });

    attachmentTable = initSimpleTable(
      "#attachmentTypesTable",
      apis.attachment,
      [
        { data: "code" },
        { data: "name" },
        {
          data: "isRequired",
          render: function (data, type, row) {
            // Handle berbagai kemungkinan: true, 1, "true", "True"
            const isRequired =
              data === true || data === 1 || data === "true" || data === "True";
            return isRequired
              ? '<span class="badge bg-success">Wajib</span>'
              : '<span class="badge bg-secondary">Tidak Wajib</span>';
          },
        },
        { data: "appliesTo" },
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
          },
        },
      ],
      {
        searchPlaceholder: "Search attachment types... ",
        infoText: "Showing _START_ to _END_ of _TOTAL_ attachment types",
      },
    );

    $("#btnCreateAttachmentType").on("click", function () {
      $("#attachmentTypeId").val("");
      $("#attachmentCode").val("");
      $("#attachmentName").val("");
      $("#attachmentAppliesTo").val("");
      $("#attachmentIsRequired").prop("checked", true);
      updateAttachmentIsRequiredLabel();
      $("#attachmentModalLabel").text("Create Attachment Type");
      showModal("attachmentModal");
    });

    $("#attachmentTypesTable").on("click", ".btn-attachment-edit", function () {
      const id = $(this).data("id");
      authFetch(`${apis.attachment}/${id}`)
        .then((r) => (r.ok ? r.json() : Promise.reject(r)))
        .then((data) => {
          console.log("[DEBUG] Edit Attachment - Loaded data:", data);
          console.log("[DEBUG] isRequired value:", data.isRequired);

          $("#attachmentModalLabel").text("Edit Attachment Type");
          $("#attachmentTypeId").val(data.attachmentTypeId);
          $("#attachmentCode").val(data.code);
          $("#attachmentName").val(data.name);
          $("#attachmentAppliesTo").val(data.appliesTo);

          // Set checkbox dengan explicit boolean conversion
          const isReq = data.isRequired === true;
          $("#attachmentIsRequired").prop("checked", isReq);
          updateAttachmentIsRequiredLabel();

          console.log("[DEBUG] Checkbox set to:", isReq);
          showModal("attachmentModal");
        })
        .catch(() =>
          swalError("Error", { message: "Unable to load attachment type." }),
        );
    });

    $("#attachmentTypesTable").on(
      "click",
      ".btn-attachment-delete",
      function () {
        const id = $(this).data("id");
        confirmDelete("attachment type").then((result) => {
          if (!result.isConfirmed) return;
          authFetch(`${apis.attachment}/${id}`, { method: "DELETE" })
            .then((r) => {
              if (r.status === 204 || r.ok) return;
              return safeRejectJson(r);
            })
            .then(() => {
              attachmentTable.ajax.reload(null, false);
              Swal.fire("Deleted", "Attachment type deleted.", "success");
            })
            .catch((err) => swalError("Error", err));
        });
      },
    );

    $("#attachmentForm").on("submit", function (e) {
      e.preventDefault();
      const id = $("#attachmentTypeId").val();

      // Get checkbox value explicitly
      const checkbox = document.getElementById("attachmentIsRequired");
      const isRequiredValue = checkbox ? checkbox.checked : false;

      const payload = {
        attachmentTypeId: id ? Number(id) : undefined,
        code: $("#attachmentCode").val(),
        name: $("#attachmentName").val(),
        appliesTo: $("#attachmentAppliesTo").val(),
        isRequired: isRequiredValue,
      };

      const method = id ? "PUT" : "POST";
      const url = id ? `${apis.attachment}/${id}` : apis.attachment;

      authFetch(url, {
        method,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      })
        .then((r) => {
          if (r.status === 201 || r.status === 204 || r.ok)
            return r.json().catch(() => ({}));
          return safeRejectJson(r);
        })
        .then(() => {
          hideModal("attachmentModal");
          attachmentTable.ajax.reload(null, false);
          Swal.fire("Saved", "Attachment type saved.", "success");
        })
        .catch((err) => {
          console.error("[DEBUG] Error occurred:", err);
          swalError("Error", err);
        });
    });

    const cols = [
      { header: "Code", value: (r) => r.code },
      { header: "Name", value: (r) => r.name },
      {
        header: "Required",
        value: (r) => (r.isRequired ? "Wajib" : "Tidak Wajib"),
      },
      { header: "Applies To", value: (r) => r.appliesTo },
    ];

    $("#btnExportAttachmentCsv").on("click", function (e) {
      e.preventDefault();
      exportToCsv(attachmentTable, cols, "attachment_types");
    });
    $("#btnExportAttachmentExcel").on("click", function (e) {
      e.preventDefault();
      exportToExcelHtml(attachmentTable, cols, "attachment_types");
    });
  }

  function wireContract() {
    contractTable = initSimpleTable(
      "#contractStatusesTable",
      apis.contract,
      [
        { data: "code" },
        { data: "name" },
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
          },
        },
      ],
      {
        searchPlaceholder: "Search contract statuses... ",
        infoText: "Showing _START_ to _END_ of _TOTAL_ contract statuses",
      },
    );

    $("#btnCreateContractStatus").on("click", function () {
      $("#contractStatusId").val("");
      $("#contractCode").val("");
      $("#contractName").val("");
      $("#contractModalLabel").text("Create Contract Status");
      showModal("contractModal");
    });

    $("#contractStatusesTable").on("click", ".btn-contract-edit", function () {
      const id = $(this).data("id");
      authFetch(`${apis.contract}/${id}`)
        .then((r) => (r.ok ? r.json() : Promise.reject(r)))
        .then((data) => {
          $("#contractModalLabel").text("Edit Contract Status");
          $("#contractStatusId").val(data.contractStatusId);
          $("#contractCode").val(data.code);
          $("#contractName").val(data.name);
          showModal("contractModal");
        })
        .catch(() =>
          swalError("Error", { message: "Unable to load contract status." }),
        );
    });

    $("#contractStatusesTable").on(
      "click",
      ".btn-contract-delete",
      function () {
        const id = $(this).data("id");
        confirmDelete("contract status").then((result) => {
          if (!result.isConfirmed) return;
          authFetch(`${apis.contract}/${id}`, { method: "DELETE" })
            .then((r) => {
              if (r.status === 204 || r.ok) return;
              return safeRejectJson(r);
            })
            .then(() => {
              contractTable.ajax.reload(null, false);
              Swal.fire("Deleted", "Contract status deleted.", "success");
            })
            .catch((err) => swalError("Error", err));
        });
      },
    );

    $("#contractForm").on("submit", function (e) {
      e.preventDefault();
      const id = $("#contractStatusId").val();
      const payload = {
        contractStatusId: id ? Number(id) : undefined,
        code: $("#contractCode").val(),
        name: $("#contractName").val(),
      };
      const method = id ? "PUT" : "POST";
      const url = id ? `${apis.contract}/${id}` : apis.contract;
      authFetch(url, {
        method,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      })
        .then((r) => {
          if (r.status === 201 || r.status === 204 || r.ok)
            return r.json().catch(() => ({}));
          return safeRejectJson(r);
        })
        .then(() => {
          hideModal("contractModal");
          contractTable.ajax.reload(null, false);
          Swal.fire("Saved", "Contract status saved.", "success");
        })
        .catch((err) => swalError("Error", err));
    });

    const cols = [
      { header: "Code", value: (r) => r.code },
      { header: "Name", value: (r) => r.name },
    ];

    $("#btnExportContractCsv").on("click", function (e) {
      e.preventDefault();
      exportToCsv(contractTable, cols, "contract_status");
    });
    $("#btnExportContractExcel").on("click", function (e) {
      e.preventDefault();
      exportToExcelHtml(contractTable, cols, "contract_status");
    });
  }

  function wireInvoiceProgress() {
    invoiceProgressTable = initSimpleTable(
      "#invoiceProgressStatusesTable",
      apis.invoiceProgress,
      [
        { data: "code" },
        { data: "name" },
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
          },
        },
      ],
      {
        searchPlaceholder: "Search invoice progress statuses... ",
        infoText:
          "Showing _START_ to _END_ of _TOTAL_ invoice progress statuses",
      },
    );

    $("#btnCreateInvoiceProgressStatus").on("click", function () {
      $("#invoiceProgressStatusId").val("");
      $("#invoiceProgressCode").val("");
      $("#invoiceProgressName").val("");
      $("#invoiceProgressModalLabel").text("Create Invoice Progress Status");
      showModal("invoiceProgressModal");
    });

    $("#invoiceProgressStatusesTable").on(
      "click",
      ".btn-invoiceprogress-edit",
      function () {
        const id = $(this).data("id");
        authFetch(`${apis.invoiceProgress}/${id}`)
          .then((r) => (r.ok ? r.json() : Promise.reject(r)))
          .then((data) => {
            $("#invoiceProgressModalLabel").text(
              "Edit Invoice Progress Status",
            );
            $("#invoiceProgressStatusId").val(data.progressStatusId);
            $("#invoiceProgressCode").val(data.code);
            $("#invoiceProgressName").val(data.name);
            showModal("invoiceProgressModal");
          })
          .catch(() =>
            swalError("Error", {
              message: "Unable to load invoice progress status.",
            }),
          );
      },
    );

    $("#invoiceProgressStatusesTable").on(
      "click",
      ".btn-invoiceprogress-delete",
      function () {
        const id = $(this).data("id");
        confirmDelete("invoice progress status").then((result) => {
          if (!result.isConfirmed) return;
          authFetch(`${apis.invoiceProgress}/${id}`, { method: "DELETE" })
            .then((r) => {
              if (r.status === 204 || r.ok) return;
              return safeRejectJson(r);
            })
            .then(() => {
              invoiceProgressTable.ajax.reload(null, false);
              Swal.fire(
                "Deleted",
                "Invoice progress status deleted.",
                "success",
              );
            })
            .catch((err) => swalError("Error", err));
        });
      },
    );

    $("#invoiceProgressForm").on("submit", function (e) {
      e.preventDefault();
      const id = $("#invoiceProgressStatusId").val();
      const payload = {
        progressStatusId: id ? Number(id) : undefined,
        code: $("#invoiceProgressCode").val(),
        name: $("#invoiceProgressName").val(),
      };
      const method = id ? "PUT" : "POST";
      const url = id ? `${apis.invoiceProgress}/${id}` : apis.invoiceProgress;
      authFetch(url, {
        method,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      })
        .then((r) => {
          if (r.status === 201 || r.status === 204 || r.ok)
            return r.json().catch(() => ({}));
          return safeRejectJson(r);
        })
        .then(() => {
          hideModal("invoiceProgressModal");
          invoiceProgressTable.ajax.reload(null, false);
          Swal.fire("Saved", "Invoice progress status saved.", "success");
        })
        .catch((err) => swalError("Error", err));
    });

    const cols = [
      { header: "Code", value: (r) => r.code },
      { header: "Name", value: (r) => r.name },
    ];

    $("#btnExportInvoiceProgressCsv").on("click", function (e) {
      e.preventDefault();
      exportToCsv(invoiceProgressTable, cols, "invoice_progress_status");
    });
    $("#btnExportInvoiceProgressExcel").on("click", function (e) {
      e.preventDefault();
      exportToExcelHtml(invoiceProgressTable, cols, "invoice_progress_status");
    });
  }

  function wireVendorCategory() {
    // Load budget codes into the dropdown
    authFetch(`${apis.budgetCode}?page=1&pageSize=2000`)
      .then((r) => (r.ok ? r.json() : Promise.reject(r)))
      .then((json) => {
        const items = normalizeToArray(json);
        const $sel = $("#vendorCategoryBudgetCode");
        $sel.find("option:not(:first)").remove();
        items.forEach((b) => {
          $sel.append(
            `<option value="${b.budgetCodeId}">${b.code} - ${b.description}</option>`,
          );
        });
      })
      .catch(() => {});

    // Initialize Select2 on Budget Code dropdown
    $("#vendorCategoryBudgetCode").select2({
      theme: "bootstrap-5",
      placeholder: "-- Search or Select Budget Code --",
      allowClear: true,
      width: "100%",
      dropdownParent: $("#vendorCategoryModal"),
    });

    vendorCategoryTable = $("#vendorCategoriesTable").DataTable({
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
        url: `${apis.vendorCategory}?page=1&pageSize=2000`,
        dataSrc: function (json) {
          return normalizeToArray(json);
        },
      },
      columns: [
        {
          data: "noCoa",
          render: function (data) {
            return data || '<span class="text-muted">-</span>';
          },
        },
        {
          data: "parentBudgetCodeLabel",
          render: function (data) {
            return data || '<span class="text-muted">-</span>';
          },
        },
        {
          data: "name",
        },
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
          },
        },
      ],
      pageLength: 10,
      lengthMenu: [10, 20, 50, 100, 200],
      pagingType: "simple_numbers",
      scrollX: false,
      autoWidth: false,
      language: {
        search: "",
        searchPlaceholder: "Search vendor categories... ",
        lengthMenu: "_MENU_ / page",
        info: "Showing _START_ to _END_ of _TOTAL_ vendor categories",
        infoEmpty: "No items found",
        zeroRecords: "No matching items",
        paginate: {
          previous: '<i class="feather-chevron-left"></i>',
          next: '<i class="feather-chevron-right"></i>',
        },
      },
      columnDefs: [
        { targets: 0, width: "120px" },
        { targets: 1, width: "180px" },
        { targets: 2 },
        {
          targets: -1,
          width: "200px",
          className: "text-center text-nowrap dt-actions",
        },
      ],
    });

    $("#btnCreateVendorCategory").on("click", function () {
      $("#vendorCategoryId").val("");
      $("#vendorCategoryNoCoa").val("");
      $("#vendorCategoryBudgetCode").val("").trigger("change");
      $("#vendorCategoryName").val("");
      $("#vendorCategoryModalLabel").text("Create Vendor Category");
      showModal("vendorCategoryModal");
    });

    $("#vendorCategoriesTable").on("click", ".btn-vendorcat-edit", function () {
      const id = $(this).data("id");
      authFetch(`${apis.vendorCategory}/${id}`)
        .then((r) => (r.ok ? r.json() : Promise.reject(r)))
        .then((data) => {
          $("#vendorCategoryModalLabel").text("Edit Vendor Category");
          $("#vendorCategoryId").val(data.vendorCategoryId);
          $("#vendorCategoryNoCoa").val(data.noCoa);
          $("#vendorCategoryBudgetCode")
            .val(data.parentBudgetCodeId || "")
            .trigger("change");
          $("#vendorCategoryName").val(data.name);
          showModal("vendorCategoryModal");
        })
        .catch(() =>
          swalError("Error", { message: "Unable to load vendor category." }),
        );
    });

    $("#vendorCategoriesTable").on(
      "click",
      ".btn-vendorcat-delete",
      function () {
        const id = $(this).data("id");
        confirmDelete("vendor category").then((result) => {
          if (!result.isConfirmed) return;
          authFetch(`${apis.vendorCategory}/${id}`, { method: "DELETE" })
            .then((r) => {
              if (r.status === 204 || r.ok) return;
              return safeRejectJson(r);
            })
            .then(() => {
              vendorCategoryTable.ajax.reload(null, false);
              Swal.fire("Deleted", "Vendor category deleted.", "success");
            })
            .catch((err) => swalError("Error", err));
        });
      },
    );

    $("#vendorCategoryForm").on("submit", function (e) {
      e.preventDefault();
      const id = $("#vendorCategoryId").val();
      const budgetCodeId = $("#vendorCategoryBudgetCode").val();
      const payload = {
        vendorCategoryId: id ? Number(id) : undefined,
        noCoa: $("#vendorCategoryNoCoa").val() || null,
        parentBudgetCodeId: budgetCodeId || null,
        name: $("#vendorCategoryName").val(),
      };
      const method = id ? "PUT" : "POST";
      const url = id ? `${apis.vendorCategory}/${id}` : apis.vendorCategory;
      authFetch(url, {
        method,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      })
        .then((r) => {
          if (r.status === 201 || r.status === 204 || r.ok)
            return r.json().catch(() => ({}));
          return safeRejectJson(r);
        })
        .then(() => {
          hideModal("vendorCategoryModal");
          vendorCategoryTable.ajax.reload(null, false);
          Swal.fire("Saved", "Vendor category saved.", "success");
        })
        .catch((err) => swalError("Error", err));
    });

    const cols = [
      { header: "No COA", value: (r) => r.noCoa },
      { header: "Budget Code", value: (r) => r.parentBudgetCodeLabel },
      { header: "Name", value: (r) => r.name },
    ];

    $("#btnExportVendorCategoryCsv").on("click", function (e) {
      e.preventDefault();
      exportToCsv(vendorCategoryTable, cols, "vendor_categories");
    });
    $("#btnExportVendorCategoryExcel").on("click", function (e) {
      e.preventDefault();
      exportToExcelHtml(vendorCategoryTable, cols, "vendor_categories");
    });
  }

  function wireBudgetCode() {
    budgetCodeTable = initSimpleTable(
      "#budgetCodesTable",
      apis.budgetCode,
      [
        { data: "code" },
        { data: "description" },
        {
          data: null,
          orderable: false,
          searchable: false,
          render: function (data, type, row) {
            return `
              <div class="hstack gap-1 justify-content-center flex-nowrap">
                <button type="button" class="btn btn-sm btn-light-brand btn-budgetcode-edit" data-id="${row.budgetCodeId}">
                  <i class="feather-edit-2 me-1"></i> Edit
                </button>
                <button type="button" class="btn btn-sm btn-light-danger btn-budgetcode-delete" data-id="${row.budgetCodeId}">
                  <i class="feather-trash-2 me-1"></i> Delete
                </button>
              </div>
            `;
          },
        },
      ],
      {
        searchPlaceholder: "Search budget codes... ",
        infoText: "Showing _START_ to _END_ of _TOTAL_ budget codes",
      },
    );

    $("#btnCreateBudgetCode").on("click", function () {
      $("#budgetCodeId").val("");
      $("#budgetCodeCode").val("");
      $("#budgetCodeDescription").val("").removeClass("is-invalid");
      $("#budgetCodeDescriptionError").addClass("d-none").text("");
      $("#budgetCodeModalLabel").text("Create Budget Code");
      showModal("budgetCodeModal");
    });

    $("#budgetCodesTable").on("click", ".btn-budgetcode-edit", function () {
      const id = $(this).data("id");
      authFetch(`${apis.budgetCode}/${id}`)
        .then((r) => (r.ok ? r.json() : Promise.reject(r)))
        .then((data) => {
          $("#budgetCodeModalLabel").text("Edit Budget Code");
          $("#budgetCodeId").val(data.budgetCodeId);
          $("#budgetCodeCode").val(data.code);
          $("#budgetCodeDescription")
            .val(data.description)
            .removeClass("is-invalid");
          $("#budgetCodeDescriptionError").addClass("d-none").text("");
          showModal("budgetCodeModal");
        })
        .catch(() =>
          swalError("Error", { message: "Unable to load budget code." }),
        );
    });

    $("#budgetCodesTable").on("click", ".btn-budgetcode-delete", function () {
      const id = $(this).data("id");
      confirmDelete("budget code").then((result) => {
        if (!result.isConfirmed) return;
        authFetch(`${apis.budgetCode}/${id}`, { method: "DELETE" })
          .then((r) => {
            if (r.status === 204 || r.ok) return;
            return safeRejectJson(r);
          })
          .then(() => {
            budgetCodeTable.ajax.reload(null, false);
            Swal.fire("Deleted", "Budget code deleted.", "success");
          })
          .catch((err) => swalError("Error", err));
      });
    });

    $("#budgetCodeDescription").on("input", function () {
      const currentId = $("#budgetCodeId").val();
      const val = ($(this).val() || "").trim().toLowerCase();
      const $err = $("#budgetCodeDescriptionError");
      if (!val) {
        $(this).removeClass("is-invalid");
        $err.addClass("d-none").text("");
        return;
      }
      const rows = budgetCodeTable
        ? budgetCodeTable.rows().data().toArray()
        : [];
      const dup = rows.find(
        (r) =>
          r.description &&
          r.description.toLowerCase() === val &&
          String(r.budgetCodeId) !== String(currentId),
      );
      if (dup) {
        $(this).addClass("is-invalid");
        $err
          .removeClass("d-none")
          .text(`Description "${dup.description}" sudah terdaftar.`);
      } else {
        $(this).removeClass("is-invalid");
        $err.addClass("d-none").text("");
      }
    });

    $("#budgetCodeForm").on("submit", function (e) {
      e.preventDefault();
      const id = $("#budgetCodeId").val();
      const descVal = ($("#budgetCodeDescription").val() || "")
        .trim()
        .toLowerCase();
      const allRows = budgetCodeTable
        ? budgetCodeTable.rows().data().toArray()
        : [];
      const dupDesc = allRows.find(
        (r) =>
          r.description &&
          r.description.toLowerCase() === descVal &&
          String(r.budgetCodeId) !== String(id),
      );
      if (dupDesc) {
        $("#budgetCodeDescription").addClass("is-invalid");
        $("#budgetCodeDescriptionError")
          .removeClass("d-none")
          .text(`Description "${dupDesc.description}" sudah terdaftar.`);
        return;
      }
      const payload = {
        budgetCodeId: id || undefined,
        code: $("#budgetCodeCode").val(),
        description: $("#budgetCodeDescription").val(),
      };
      const method = id ? "PUT" : "POST";
      const url = id ? `${apis.budgetCode}/${id}` : apis.budgetCode;
      authFetch(url, {
        method,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      })
        .then((r) => {
          if (r.status === 201 || r.status === 204 || r.ok)
            return r.json().catch(() => ({}));
          return safeRejectJson(r);
        })
        .then(() => {
          hideModal("budgetCodeModal");
          budgetCodeTable.ajax.reload(null, false);
          Swal.fire("Saved", "Budget code saved.", "success");
        })
        .catch((err) => swalError("Error", err));
    });

    const cols = [
      { header: "Code", value: (r) => r.code },
      { header: "Description", value: (r) => r.description },
    ];

    $("#btnExportBudgetCodeCsv").on("click", function (e) {
      e.preventDefault();
      exportToCsv(budgetCodeTable, cols, "budget_codes");
    });
    $("#btnExportBudgetCodeExcel").on("click", function (e) {
      e.preventDefault();
      exportToExcelHtml(budgetCodeTable, cols, "budget_codes");
    });
  }

  function wireTabAdjustments() {
    document.querySelectorAll('button[data-bs-toggle="tab"]').forEach((btn) => {
      btn.addEventListener("shown.bs.tab", function (event) {
        const target = event.target.getAttribute("data-bs-target");
        // Adjust columns for tables inside newly shown tabs
        if (target === "#tab-approval" && approvalTable)
          approvalTable.columns.adjust();
        if (target === "#tab-attachment" && attachmentTable)
          attachmentTable.columns.adjust();
        if (target === "#tab-contract" && contractTable)
          contractTable.columns.adjust();
        if (target === "#tab-invoice-progress" && invoiceProgressTable)
          invoiceProgressTable.columns.adjust();
        if (target === "#tab-vendor-category" && vendorCategoryTable)
          vendorCategoryTable.columns.adjust();
        if (target === "#tab-budget-code" && budgetCodeTable)
          budgetCodeTable.columns.adjust();
      });
    });
  }

  $(function () {
    // Fix template blur by ensuring modals are direct children of <body>
    portalModalToBody("approvalModal");
    portalModalToBody("attachmentModal");
    portalModalToBody("contractModal");
    portalModalToBody("invoiceProgressModal");
    portalModalToBody("advancedStatusModal");
    portalModalToBody("biayaRealisasiStatusModal");
    portalModalToBody("vendorCategoryModal");
    portalModalToBody("budgetCodeModal");

    wireApproval();
    wireAttachment();
    wireContract();
    wireInvoiceProgress();
    wireAdvancedStatus();
    wireBiayaRealisasiStatus();
    wireVendorCategory();
    wireBudgetCode();
    wireTabAdjustments();
  });
})(jQuery);
