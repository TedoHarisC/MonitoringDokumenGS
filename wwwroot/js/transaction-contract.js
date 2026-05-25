(function ($) {
  "use strict";

  const apis = {
    contracts: "/api/contracts",
    vendors: "/api/vendors?page=1&pageSize=2000",
    contractStatuses: "/api/contract-statuses?page=1&pageSize=2000",
    approvalStatuses: "/api/approval-statuses?page=1&pageSize=2000",
    attachments: "/api/attachments",
    currentUser: "/api/auth/me",
  };

  let table;
  let cachedVendors = [];
  let cachedContractStatuses = [];
  let cachedApprovalStatuses = [];
  let pendingFiles = [];
  let currentUserVendor = null;

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

  async function fetchJson(url, options) {
    const res = await authFetch(url, options);
    if (res.status === 204) return null;
    if (!res.ok) {
      let body = null;
      try {
        body = await res.json();
      } catch {
        body = { message: `Request failed (${res.status})` };
      }
      throw body;
    }
    try {
      return await res.json();
    } catch {
      return null;
    }
  }

  function escapeHtml(value) {
    if (value === null || value === undefined) return "";
    return String(value)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#39;");
  }

  function toGuidString(value) {
    if (!value) return "";
    return String(value);
  }

  function toInt(value) {
    const n = Number(value);
    return Number.isFinite(n) ? n : 0;
  }

  function formatDate(value) {
    if (!value) return "";
    const d = new Date(value);
    if (isNaN(d.getTime())) return String(value);
    const yyyy = d.getFullYear();
    const mm = String(d.getMonth() + 1).padStart(2, "0");
    const dd = String(d.getDate()).padStart(2, "0");
    return `${yyyy}-${mm}-${dd}`;
  }

  function formatDateTime(value) {
    if (!value) return "";
    const d = new Date(value);
    if (isNaN(d.getTime())) return String(value);
    return d.toLocaleString();
  }

  function currentUserId() {
    const el = document.getElementById("currentUserId");
    const v = el ? String(el.value || "").trim() : "";
    return v;
  }

  function isCurrentUserAdmin() {
    const el = document.getElementById("currentUserIsAdmin");
    const v = el
      ? String(el.value || "")
        .trim()
        .toLowerCase()
      : "false";
    return v === "true";
  }

  async function loadCurrentUser() {
    const result = await fetchJson(apis.currentUser, {
      credentials: "same-origin",
    });
    currentUserVendor = {
      vendorId: result.vendorId,
      vendorName: result.vendorName,
    };
  }

  function vendorNameById(id) {
    const gid = toGuidString(id);
    if (!gid) return null;

    // First check current user vendor
    if (
      currentUserVendor &&
      String(currentUserVendor.vendorId).toLowerCase() ===
      String(gid).toLowerCase().trim()
    ) {
      return currentUserVendor.vendorName;
    }

    const found = cachedVendors.find(
      (v) =>
        String(v.vendorId || v.VendorId).toLowerCase() === gid.toLowerCase(),
    );
    if (!found) return null;
    return String(found.vendorName || found.VendorName || "").trim() || null;
  }

  function contractStatusNameById(id) {
    const sid = toInt(id);
    if (!sid) return null;
    const found = cachedContractStatuses.find(
      (s) => toInt(s.contractStatusId || s.ContractStatusId) === sid,
    );
    if (!found) return null;
    return (
      String(
        found.name || found.Name || found.code || found.Code || "",
      ).trim() || null
    );
  }

  function approvalStatusNameById(id) {
    const sid = toInt(id);
    if (!sid) return null;
    const found = cachedApprovalStatuses.find(
      (s) => toInt(s.approvalStatusId || s.ApprovalStatusId) === sid,
    );
    if (!found) return null;
    return (
      String(
        found.name || found.Name || found.code || found.Code || "",
      ).trim() || null
    );
  }

  function getVendorColor(vendorId) {
    // Array of Bootstrap badge color classes
    const colors = [
      "primary",
      "success",
      "info",
      "warning",
      "danger",
      "secondary",
      "dark",
    ];

    // Generate consistent color based on vendor ID
    const id = String(vendorId || "").toLowerCase();
    let hash = 0;
    for (let i = 0; i < id.length; i++) {
      hash = id.charCodeAt(i) + ((hash << 5) - hash);
    }
    const index = Math.abs(hash) % colors.length;
    return colors[index];
  }

  function getContractStatusColor(statusId) {
    // Map contract status IDs to colors
    const statusColors = {
      1: "secondary", // e.g., Draft
      2: "info", // e.g., Active
      3: "success", // e.g., Completed
      4: "warning", // e.g., On Hold
      5: "danger", // e.g., Terminated
      6: "primary", // e.g., Renewed
    };
    return statusColors[statusId] || "secondary";
  }

  function getApprovalStatusColor(statusId) {
    // Map approval status IDs to colors
    const statusColors = {
      6: "warning", // e.g., Draft
      7: "secondary", // e.g., Pending Review
      8: "info", // e.g., In Review
      9: "primary", // e.g., Approved
      10: "success", // e.g., Rejected
    };
    return statusColors[statusId] || "secondary";
  }

  function setSelectOptions(selectEl, options, placeholder) {
    const ph = placeholder
      ? `<option value="">${escapeHtml(placeholder)}</option>`
      : "";
    selectEl.innerHTML = ph + options.join("");
  }

  async function loadVendors() {
    const result = await fetchJson(apis.vendors);
    cachedVendors = normalizeToArray(result);
    return cachedVendors;
  }

  async function loadContractStatuses() {
    const result = await fetchJson(apis.contractStatuses);
    cachedContractStatuses = normalizeToArray(result);
    return cachedContractStatuses;
  }

  async function loadApprovalStatuses() {
    const result = await fetchJson(apis.approvalStatuses);
    cachedApprovalStatuses = normalizeToArray(result);
    return cachedApprovalStatuses;
  }

  function populateContractStatusDropdown() {
    const $select = $("#contractStatusSelect");
    $select.empty();
    $select.append('<option value="">-- Select Contract Status --</option>');

    cachedContractStatuses.forEach((status) => {
      const id = status.contractStatusId || status.ContractStatusId;
      const name = status.name || status.Name || status.code || status.Code;
      $select.append(`<option value="${id}">${escapeHtml(name)}</option>`);
    });
  }

  function populateApprovalStatusDropdown() {
    const $select = $("#approvalStatusSelect");
    $select.empty();
    $select.append('<option value="">-- Select Approval Status --</option>');

    cachedApprovalStatuses.forEach((status) => {
      const id = status.approvalStatusId || status.ApprovalStatusId;
      const name = status.name || status.Name || status.code || status.Code;
      $select.append(`<option value="${id}">${escapeHtml(name)}</option>`);
    });
  }

  function populateFormSelects() {
    const vendorSelect = document.getElementById("vendorId");
    const contractStatusSelect = document.getElementById("contractStatusId");
    const approvalStatusSelect = document.getElementById("approvalStatusId");

    const vendorOptions = cachedVendors.map((v) => {
      const id = String(v.vendorId || v.VendorId || "");
      const name = String(v.vendorName || v.VendorName || id);
      return `<option value="${escapeHtml(id)}">${escapeHtml(name)}</option>`;
    });

    const contractStatusOptions = cachedContractStatuses.map((s) => {
      const id = String(s.contractStatusId || s.ContractStatusId || "");
      const label = `${String(s.name || s.Name || "")}`.trim();
      return `<option value="${escapeHtml(id)}">${escapeHtml(label)}</option>`;
    });

    const approvalStatusOptions = cachedApprovalStatuses.map((s) => {
      const id = String(s.approvalStatusId || s.ApprovalStatusId || "");
      const label = `${String(s.name || s.Name || "")}`.trim();
      return `<option value="${escapeHtml(id)}">${escapeHtml(label)}</option>`;
    });

    setSelectOptions(vendorSelect, vendorOptions, "-- Select Vendor --");
    setSelectOptions(
      contractStatusSelect,
      contractStatusOptions,
      "-- Select Contract Status --",
    );
    setSelectOptions(
      approvalStatusSelect,
      approvalStatusOptions,
      "-- Select Approval Status --",
    );
  }

  function initTable() {
    table = $("#contractsTable").DataTable({
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
      initComplete: function () {
        const $target = $("#contractExportActions");
        if ($target.length) {
          $target.empty();
          $(table.buttons().container()).appendTo($target);
        }
      },
      buttons: [
        {
          extend: "excelHtml5",
          text: '<i class="feather-download me-1"></i> Export Excel',
          className: "btn btn-success btn-sm",
          title: "Contracts",
          filename: function () {
            const today = new Date();
            const yyyy = today.getFullYear();
            const mm = String(today.getMonth() + 1).padStart(2, "0");
            const dd = String(today.getDate()).padStart(2, "0");
            return `rekap-contract-${yyyy}-${mm}-${dd}`;
          },
          exportOptions: {
            columns: [0, 1, 2, 3, 4, 5, 6],
            format: {
              body: function (data) {
                return $("<div>").html(data).text().trim();
              },
            },
          },
        },
      ],
      ajax: {
        url: apis.contracts,
        data: function (d) {
          d.vendorId = $('#filterVendor').val();
          d.contractStatusId = $('#filterContractStatus').val();
          d.approvalStatusId = $('#filterApprovalStatus').val();
          d.startDate = $('#filterStartDate').val();
          d.endDate = $('#filterEndDate').val();
        },
        dataSrc: function (json) {
          return normalizeToArray(json);
        },
      },
      columns: [
        { data: "contractNumber" },
        {
          data: "vendorName",
          render: function (data) {
            const vendorName = vendorNameById(data) || data || "Unknown";
            const colorClass = getVendorColor(data);
            return `<span class="badge bg-${colorClass} bg-${colorClass} text-white" style="font-size: 0.875rem; padding: 0.35rem 0.75rem;">${escapeHtml(data)}</span>`;
          },
        },
        {
          data: "contractDescription",
          render: function (data) {
            const txt = String(data || "");
            return txt.length > 50
              ? escapeHtml(txt.substring(0, 50)) + "..."
              : escapeHtml(txt);
          },
        },
        {
          data: "startDate",
          render: function (data) {
            return escapeHtml(formatDate(data));
          },
        },
        {
          data: "endDate",
          render: function (data) {
            return escapeHtml(formatDate(data));
          },
        },
        {
          data: null,
          className: "text-center",
          render: function (data, type, row) {
            const validityStatus = row.validityStatus || "";
            const daysUntilExpiry = row.daysUntilExpiry || 0;

            let badgeClass = "bg-success";
            let icon = "feather-check-circle";
            let statusText = "Active";
            let tooltipText = `${daysUntilExpiry} days remaining`;

            if (validityStatus === "Expired") {
              badgeClass = "bg-danger";
              icon = "feather-x-circle";
              statusText = "Expired";
              tooltipText = `Expired ${Math.abs(daysUntilExpiry)} days ago`;
            } else if (validityStatus === "Expiring Soon") {
              badgeClass = "bg-warning";
              icon = "feather-alert-circle";
              statusText = "Expiring Soon";
              tooltipText = `Expires in ${daysUntilExpiry} days`;
            }

            return `<span class="badge ${badgeClass} text-white d-inline-flex align-items-center gap-1" style="font-size: 0.875rem; padding: 0.35rem 0.75rem;" data-bs-toggle="tooltip" title="${escapeHtml(tooltipText)}">
                            <i class="${icon}" style="font-size: 0.9rem;"></i>
                            ${escapeHtml(statusText)}
                        </span>`;
          },
        },
        // {
        //     data: 'contractStatusId',
        //     render: function (data) {
        //         const statusName = contractStatusNameById(data) || data || 'Unknown'
        //         const colorClass = getContractStatusColor(data)
        //         return `<span class="fw-bold text-${colorClass}" style="font-size: 0.9rem;">${escapeHtml(statusName)}</span>`
        //     }
        // },
        {
          data: "approvalStatusId",
          render: function (data) {
            const statusName =
              approvalStatusNameById(data) || data || "Unknown";
            const colorClass = getApprovalStatusColor(data);
            return `<span class="badge bg-${colorClass} text-white" style="font-size: 0.875rem; padding: 0.35rem 0.75rem;">${escapeHtml(statusName)}</span>`;
          },
        },
        {
          data: null,
          orderable: false,
          searchable: false,
          render: function (data, type, row) {
            const id = row.contractId || row.ContractId;
            return `
                <div class="hstack gap-1 justify-content-center flex-nowrap">
                    <button type="button" class="btn btn-sm btn-light-info btn-detail" data-id="${escapeHtml(id)}">
                        <i class="feather-eye me-1"></i> Detail
                    </button>
                    <button type="button" class="btn btn-sm btn-light-brand btn-edit" data-id="${escapeHtml(id)}">
                        <i class="feather-edit-2 me-1"></i> Edit
                    </button>
                    <button type="button" class="btn btn-sm btn-light-danger btn-delete" data-id="${escapeHtml(id)}">
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
      scrollX: true,
      autoWidth: false,
      language: {
        search: "",
        searchPlaceholder: "Search contracts... ",
        lengthMenu: "_MENU_ / page",
        info: "Showing _START_ to _END_ of _TOTAL_ contracts",
        infoEmpty: "No contracts found",
        zeroRecords: "No matching contracts",
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
      drawCallback: function () {
        // Initialize tooltips after table draw
        $('[data-bs-toggle="tooltip\"]').tooltip();
      },
    });
  }

  function clearForm() {
    $("#contractId").val("");
    $("#vendorId").val("");
    $("#vendorName").val("");
    $("#contractNumber").val("");
    $("#contractDescription").val("");
    $("#startDate").val("");
    $("#endDate").val("");
    $("#contractStatusId").val("2");
    $("#contractStatusSelect").val("2");
    $("#approvalStatusId").val("6");
    $("#approvalStatusSelect").val("6");
  }

  // Untuk Setup Vendor Field di form (Create/Edit) berdasarkan role user
  function setupVendorField(vendorId, vendorName) {
    if (isCurrentUserAdmin()) {
      // ADMIN → pakai select
      $("#vendorIdSelect").show();
      $("#vendorName").hide();

      populateVendorDropdown();

      $("#vendorIdSelect").val(String(vendorId || ""));
    } else {
      // USER → readonly
      $("#vendorIdSelect").hide();
      $("#vendorName").show();

      $("#vendorId").val(String(vendorId || ""));
      $("#vendorName").val(vendorName || "");
    }
  }

  function openCreate() {
    portalModalToBody("contractModal");
    clearForm();
    $("#contractModalLabel").text("Create Contract");

    // Set vendor from current user
    // if (currentUserVendor && currentUserVendor.vendorId) {
    //   $("#vendorId").val(currentUserVendor.vendorId);
    //   $("#vendorName").val(currentUserVendor.vendorName || "");
    // } else {
    //   Swal.fire(
    //     "Warning",
    //     "No vendor assigned to your account. Please contact administrator.",
    //     "warning",
    //   );
    //   return;
    // }

    // Set default statuses
    $("#contractStatusId").val("2");
    $("#contractStatusSelect").val("2");
    $("#approvalStatusId").val("6");
    $("#approvalStatusSelect").val("6");

    // Setup vendor field based on user role
    setupVendorField(currentUserVendor.vendorId, currentUserVendor.vendorName);

    // Show/hide status sections based on user role
    if (isCurrentUserAdmin()) {
      populateContractStatusDropdown();
      populateApprovalStatusDropdown();
      $("#contractStatusSection").show();
      $("#approvalStatusSection").show();
    } else {
      $("#contractStatusSection").hide();
      $("#approvalStatusSection").hide();
    }

    $("#attachmentsSection").show();
    $("#attachmentsList").html(
      '<div class="alert alert-info small">Files will be uploaded after saving the contract</div>',
    );
    pendingFiles = [];
    showModal("contractModal");
  }

  async function openEdit(id) {
    portalModalToBody("contractModal");
    const data = await fetchJson(`${apis.contracts}/${id}`);
    $("#contractModalLabel").text("Edit Contract");
    $("#contractId").val(data.contractId);

    // Set vendor
    // const vendorName = vendorNameById(data.vendorId) || "";
    // const vendorId = String(data.vendorId || "").toLowerCase();

    // // Normalize option values juga
    // $("#vendorId option").each(function () {
    //   this.value = this.value.toLowerCase();
    // });

    // $("#vendorId").val(vendorId);
    const vendorName = vendorNameById(data.vendorId) || data.vendorName || "";
    setupVendorField(data.vendorId, vendorName);
    $("#vendorName").val(vendorName);
    $("#contractNumber").val(data.contractNumber || "");
    $("#contractDescription").val(data.contractDescription || "");
    $("#startDate").val(formatDate(data.startDate));
    $("#endDate").val(formatDate(data.endDate));

    // Set status values
    $("#contractStatusId").val(String(data.contractStatusId || ""));
    $("#contractStatusSelect").val(String(data.contractStatusId || ""));
    $("#approvalStatusId").val(String(data.approvalStatusId || ""));
    $("#approvalStatusSelect").val(String(data.approvalStatusId || ""));

    // Show/hide status sections based on user role
    if (isCurrentUserAdmin()) {
      populateContractStatusDropdown();
      populateApprovalStatusDropdown();
      $("#contractStatusSection").show();
      $("#contractStatusSelect").val(String(data.contractStatusId || ""));
      $("#approvalStatusSection").show();
      $("#approvalStatusSelect").val(String(data.approvalStatusId || ""));
    } else {
      $("#contractStatusSection").hide();
      $("#approvalStatusSection").hide();
    }

    $("#attachmentsSection").show();
    await loadAttachments(data.contractId);
    showModal("contractModal");
  }

  function populateVendorDropdown() {
    const $select = $("#vendorIdSelect");
    $select.empty();

    $select.append('<option value="">-- Select Vendor --</option>');

    cachedVendors.forEach((v) => {
      const id = v.vendorId || v.VendorId;
      const name = v.vendorName || v.VendorName;

      $select.append(`<option value="${id}">${name}</option>`);
    });
  }

  async function saveContract(e) {
    e.preventDefault();

    const id = String($("#contractId").val() || "").trim();
    const uid = currentUserId();
    //const vendorId = String($("#vendorId").val() || "").trim();
    const isEdit = !!id;

    // Get statuses - from select if admin, otherwise from hidden field
    const contractStatusId = isCurrentUserAdmin()
      ? toInt($("#contractStatusSelect").val())
      : toInt($("#contractStatusId").val());

    const approvalStatusId = isCurrentUserAdmin()
      ? toInt($("#approvalStatusSelect").val())
      : toInt($("#approvalStatusId").val());

    const vendorId = isCurrentUserAdmin()
      ? String($("#vendorIdSelect").val()).trim()
      : String($("#vendorId").val()).trim();

    const payload = {
      contractId: id || undefined,
      vendorId: vendorId,
      contractNumber: String($("#contractNumber").val() || "").trim(),
      contractDescription: String($("#contractDescription").val() || "").trim(),
      startDate: $("#startDate").val(),
      endDate: $("#endDate").val(),
      contractStatusId: contractStatusId,
      approvalStatusId: approvalStatusId,
      createdByUserId: uid || undefined,
      createdBy: uid || undefined,
      updatedBy: uid || undefined,
    };

    if (!payload.vendorId) {
      return Swal.fire("Validation", "Vendor is required.", "warning");
    }
    if (!payload.contractNumber) {
      return Swal.fire("Validation", "Contract number is required.", "warning");
    }
    if (!payload.contractDescription) {
      return Swal.fire("Validation", "Description is required.", "warning");
    }
    if (!payload.startDate || !payload.endDate) {
      return Swal.fire(
        "Validation",
        "Start and End dates are required.",
        "warning",
      );
    }
    if (!payload.contractStatusId) {
      return Swal.fire("Validation", "Contract status is required.", "warning");
    }
    if (!payload.approvalStatusId) {
      return Swal.fire("Validation", "Approval status is required.", "warning");
    }

    const method = id ? "PUT" : "POST";
    const url = id ? `${apis.contracts}/${id}` : apis.contracts;

    try {
      const result = await fetchJson(url, {
        method,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });

      // If creating new contract and have pending files, upload them
      if (!id && pendingFiles.length > 0 && result.contractId) {
        await uploadPendingFiles(result.contractId);
      }

      hideModal("contractModal");
      if (table) table.ajax.reload(null, false);
      Swal.fire("Saved", "Contract saved successfully", "success");
    } catch (err) {
      const message =
        (err && (err.message || err.title)) || "Unable to save contract.";
      Swal.fire("Error", message, "error");
    }
  }

  async function loadAttachments(contractId) {
    try {
      const attachments = await fetchJson(
        `${apis.attachments}/by-reference/${contractId}`,
      );
      const $list = $("#attachmentsList");
      $list.empty();

      if (!attachments || attachments.length === 0) {
        $list.html(
          '<div class="text-muted text-center py-3">No attachments yet</div>',
        );
        return;
      }

      attachments.forEach((att) => {
        const sizeKB = ((att.fileSize || 0) / 1024).toFixed(1);
        const fileUrl = `/api/attachments/download/${att.attachmentId}`;

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
                `);
        $list.append(item);
      });
    } catch (err) {
      console.error("Failed to load attachments:", err);
    }
  }

  async function uploadFile(file, contractId, typeId = null) {
    if (!contractId) {
      pendingFiles.push(file);
      updatePendingFilesList();
      $("#fileUploadInput").val("");
      return;
    }
    const formData = new FormData();
    formData.append("file", file);
    formData.append("module", "Contracts");
    formData.append("attachmentTypeId", typeId || "1");
    formData.append("referenceId", contractId);
    try {
      await authFetch(`${apis.attachments}/upload`, {
        method: "POST",
        body: formData,
      });
      await loadAttachments(contractId);
      $("#fileUploadInput").val("");
    } catch (err) {
      Swal.fire("Error", "Failed to upload file", "error");
    }
  }

  function updatePendingFilesList() {
    const $list = $("#attachmentsList");
    $list.empty();

    if (pendingFiles.length === 0) {
      $list.html(
        '<div class="alert alert-info small">Files will be uploaded after saving the contract</div>',
      );
      return;
    }

    $list.append(
      '<div class="alert alert-info small mb-2">Files to upload after save:</div>',
    );
    pendingFiles.forEach((pendingFile, index) => {
      const file = pendingFile.file || pendingFile;
      const typeName = pendingFile.typeName || "No Type";
      const sizeKb = (file.size / 1024).toFixed(2);
      $list.append(`
                <div class="list-group-item d-flex justify-content-between align-items-center">
                    <div>
                        <strong>${file.name}</strong>
                        <span class="badge bg-info ms-2">${typeName}</span>
                        <span class="text-muted small ms-2">(${sizeKb} KB)</span>
                    </div>
                    <button type="button" class="btn btn-sm btn-danger btn-remove-pending" data-index="${index}">
                        <i class="bi bi-x-lg"></i>
                    </button>
                </div>
            `);
    });
  }

  async function uploadPendingFiles(contractId) {
    if (pendingFiles.length === 0) return;
    for (const pendingFile of pendingFiles) {
      const file = pendingFile.file || pendingFile;
      const typeId = pendingFile.typeId || null;
      await uploadFile(file, contractId, typeId);
    }
    pendingFiles = [];
  }

  async function deleteAttachment(attachmentId, contractId) {
    const res = await Swal.fire({
      title: "Delete attachment?",
      icon: "warning",
      showCancelButton: true,
      confirmButtonText: "Yes, delete",
    });

    if (!res.isConfirmed) return;

    try {
      await authFetch(`${apis.attachments}/${attachmentId}`, {
        method: "DELETE",
      });
      Swal.fire("Deleted", "Attachment deleted", "success");
      await loadAttachments(contractId);
    } catch (err) {
      Swal.fire("Error", "Failed to delete attachment", "error");
    }
  }

  async function deleteContract(id) {
    const res = await Swal.fire({
      title: "Delete contract?",
      text: "This action cannot be undone.",
      icon: "warning",
      showCancelButton: true,
      confirmButtonText: "Delete",
      cancelButtonText: "Cancel",
    });

    if (!res.isConfirmed) return;

    try {
      await fetchJson(`${apis.contracts}/${id}`, { method: "DELETE" });
      if (table) table.ajax.reload(null, false);
      Swal.fire("Deleted", "Contract deleted", "success");
    } catch (err) {
      const message =
        (err && (err.message || err.title)) || "Unable to delete contract.";
      Swal.fire("Error", message, "error");
    }
  }

  let cachedAttachmentTypes = [];

  async function loadAttachmentTypes() {
    const result = await fetchJson(
      "/api/attachment-types?page=1&pageSize=2000",
    );
    cachedAttachmentTypes = normalizeToArray(result).filter(
      (t) => t.appliesTo === "CONTRACT",
    );
    populateAttachmentTypeDropdown();
    return cachedAttachmentTypes;
  }

  function populateAttachmentTypeDropdown() {
    const $select = $("#attachmentTypeSelect");
    $select.empty();
    $select.append('<option value="">-- Pilih Tipe Attachment --</option>');
    cachedAttachmentTypes.forEach((type) => {
      $select.append(
        `<option value="${type.attachmentTypeId}">${type.name}</option>`,
      );
    });
  }

  $(async function () {
    portalModalToBody("contractModal");

    try {
      await Promise.all([
        loadCurrentUser(),
        loadVendors(),
        loadContractStatuses(),
        loadApprovalStatuses(),
      ]);
    } catch (err) {
      console.error("Initialization error:", err);
      // still allow page to load; table will show IDs
    }

    initTable();

    $("#btnCreateContract").on("click", openCreate);

    $("#contractsTable").on("click", ".btn-edit", function () {
      const id = $(this).data("id");
      openEdit(id).catch(() =>
        Swal.fire("Error", "Unable to load contract", "error"),
      );
    });

    $("#contractsTable").on("click", ".btn-delete", function () {
      const id = $(this).data("id");
      deleteContract(id);
    });

    // Load attachment types
    await loadAttachmentTypes();

    // Add file with attachment type
    $("#btnAddFile").on("click", function () {
      const fileInput = $("#fileUploadInput")[0];
      const file = fileInput.files[0];
      const typeId = $("#attachmentTypeSelect").val();

      // Validation
      if (!file) {
        Swal.fire({
          icon: "warning",
          title: "No File Selected",
          text: "Please select a file to upload.",
        });
        return;
      }
      if (!typeId) {
        Swal.fire({
          icon: "warning",
          title: "No Type Selected",
          text: "Please select an attachment type.",
        });
        return;
      }
      // Validate file size (max 100MB)
      const maxSize = 100 * 1024 * 1024;
      if (file.size > maxSize) {
        Swal.fire({
          icon: "error",
          title: "File Too Large",
          text: "File size must not exceed 10 MB.",
        });
        return;
      }
      // Find type name
      const type = cachedAttachmentTypes.find(
        (t) => t.attachmentTypeId == typeId,
      );
      const typeName = type ? type.name : "Unknown";
      // Add to pending files
      const pendingFile = {
        file: file,
        typeId: typeId,
        typeName: typeName,
        id: Date.now(),
      };
      pendingFiles.push(pendingFile);
      updatePendingFilesList();
      // Clear inputs
      fileInput.value = "";
      $("#attachmentTypeSelect").val("");
      Swal.fire({
        icon: "success",
        title: "File Added",
        text: `${file.name} will be uploaded after saving the contract.`,
        timer: 2000,
        showConfirmButton: false,
      });
    });

    $(document).on("click", ".btn-delete-attachment", function () {
      const attachmentId = $(this).data("id");
      const contractId = $("#contractId").val();
      deleteAttachment(attachmentId, contractId);
    });

    $(document).on("click", ".btn-remove-pending", function () {
      const index = $(this).data("index");
      pendingFiles.splice(index, 1);
      updatePendingFilesList();
    });

    $("#contractForm").on("submit", saveContract);
  });

  // Handler untuk tombol detail contract
  $(document).on("click", ".btn-detail", function () {
    portalModalToBody("contractDetailModal");
    var id = $(this).data("id");
    $.get("/api/contracts/" + id, function (data) {
      var html = "";
      html +=
        '<div class="mb-2"><strong>Contract Number:</strong> ' +
        (data.contractNumber || "-") +
        "</div>";
      html +=
        '<div class="mb-2"><strong>Vendor:</strong> ' +
        (data.vendorName || "-") +
        "</div>";
      html +=
        '<div class="mb-2"><strong>Description:</strong> ' +
        (data.contractDescription || "-") +
        "</div>";
      html +=
        '<div class="mb-2"><strong>Start Date:</strong> ' +
        (data.startDate
          ? new Date(data.startDate).toLocaleDateString("id-ID")
          : "-") +
        "</div>";
      html +=
        '<div class="mb-2"><strong>End Date:</strong> ' +
        (data.endDate
          ? new Date(data.endDate).toLocaleDateString("id-ID")
          : "-") +
        "</div>";
      html += "<hr/>";
      html += '<div class="mb-2"><strong>Attachments:</strong></div>';
      $.get("/api/attachments/by-reference/" + id, function (atts) {
        if (atts && atts.length > 0) {
          html += '<ul class="list-group mb-2">';
          atts.forEach(function (att) {
            var size = att.fileSize
              ? (att.fileSize / 1024).toFixed(1) + " KB"
              : "-";
            html +=
              '<li class="list-group-item d-flex justify-content-between align-items-center">';
            html +=
              '<span><i class="feather-file me-2"></i>' +
              att.fileName +
              ' <span class="text-muted small">(' +
              size +
              ")</span></span>";
            html +=
              '<a href="/api/attachments/download/' +
              att.attachmentId +
              '" target="_blank" class="btn btn-sm btn-outline-primary"><i class="feather-download"></i> Download</a>';
            html += "</li>";
          });
          html += "</ul>";
        } else {
          html += '<div class="text-muted">No attachments.</div>';
        }
        // Timeline History Section
        html +=
          '<div class="mt-5">' +
          '<h6 class="text-primary mb-3"><i class="feather-clock me-1"></i> History Perubahan Status</h6>' +
          '<div id="contractHistoryTimeline" class="timeline-container"></div>' +
          "</div>";
        $("#contractDetailContent").html(html);
        showModal("contractDetailModal");
        // Load timeline after detail
        if (id)
          window.renderContractDetailHistory &&
            window.renderContractDetailHistory(id);
      });
    });
  });

  // Timeline History Loader for Contract Detail
  function loadContractHistory(contractId) {
    const $timeline = $("#contractHistoryTimeline");
    $timeline.html('<div class="text-muted">Loading history...</div>');
    $.get(`/api/contracts/${contractId}/history`)
      .done(function (data) {
        if (!data || !Array.isArray(data) || data.length === 0) {
          $timeline.html(
            '<div class="text-muted">Tidak ada history perubahan.</div>',
          );
          return;
        }
        // Sort by CreatedAt ascending
        data.sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt));
        let html = '<div class="timeline">';
        data.forEach(function (item, idx) {
          let status = "";
          let user =
            item.userName || (item.userId ? `User: ${item.userId}` : "-");
          let tgl = new Date(item.createdAt).toLocaleString();
          console.log("History item:", item);
          try {
            if (item.newData) {
              const newData =
                typeof item.newData === "string"
                  ? JSON.parse(item.newData)
                  : item.newData;
              status =
                newData.status ||
                newData.Status ||
                newData.contractStatusId ||
                newData.ContractStatusId ||
                "-";
            }
          } catch { }
          html += `
            <div class="timeline-item mb-4">
              <div class="d-flex align-items-center gap-2 mb-1">
                <span class="badge bg-primary"></span>
                <span class="fw-bold"></span>
                <span class="text-muted small">${item.displayText}</span>
              </div>
            </div>`;
        });
        html += "</div>";
        $timeline.html(html);
      })
      .fail(function (xhr) {
        if (
          xhr.status === 404 &&
          xhr.responseJSON &&
          xhr.responseJSON.message &&
          xhr.responseJSON.message.includes("No history")
        ) {
          $timeline.html(
            '<div class="text-muted">Tidak ada history perubahan.</div>',
          );
        } else {
          $timeline.html(
            '<div class="text-danger">Gagal memuat history perubahan.</div>',
          );
        }
      });
  }
  window.renderContractDetailHistory = function (contractId) {
    loadContractHistory(contractId);
  };

  // ─── INIT CUSTOM FILTERS ──────────────────────────────────────────────────
  function initFilters() {
    // 1. Load Contract Statuses
    $.getJSON("/api/contract-statuses?page=1&pageSize=2000").then(function (res) {
      const items = Array.isArray(res) ? res : res.items || res.data || [];
      const $sel = $('#filterContractStatus');
      $sel.empty().append('<option value="">All Status</option>');
      items.forEach(s => {
        const id = s.contractStatusId || s.ContractStatusId;
        const name = s.name || s.Name || s.code || s.Code;
        $sel.append(`<option value="${id}">${escapeHtml(name)}</option>`);
      });
      $sel.select2({ theme: "bootstrap-5", placeholder: "All Status", allowClear: true, width: "100%" });
    });

    // 2. Load Approval Statuses
    $.getJSON("/api/approval-statuses?page=1&pageSize=2000").then(function (res) {
      const items = Array.isArray(res) ? res : res.items || res.data || [];
      const $sel = $('#filterApprovalStatus');
      $sel.empty().append('<option value="">All Status</option>');
      items.forEach(s => {
        const id = s.approvalStatusId || s.ApprovalStatusId;
        const name = s.name || s.Name || s.code || s.Code;
        $sel.append(`<option value="${id}">${escapeHtml(name)}</option>`);
      });
      $sel.select2({ theme: "bootstrap-5", placeholder: "All Status", allowClear: true, width: "100%" });
    });

    // 3. Load Vendors if element exists (Admin only)
    if ($('#filterVendor').length) {
      $.getJSON("/api/vendors?page=1&pageSize=2000").then(function (res) {
        const items = Array.isArray(res) ? res : res.items || res.data || [];
        const $sel = $('#filterVendor');
        $sel.empty().append('<option value="">All Vendor</option>');
        items.forEach(function (v) {
          const id = v.vendorId || v.VendorId;
          const name = v.vendorName || v.VendorName;
          $sel.append(new Option(name, id, false, false));
        });
        $sel.select2({ theme: "bootstrap-5", placeholder: "All Vendor", allowClear: true, width: "100%" });
      });
    }
  }

  // Initialize filters
  $(function () {
    initFilters();
  });

  // ─── Filter Events ────────────────────────────────────────────────────────
  $('#btnApplyFilter').on('click', function () {
    table.ajax.reload();
  });

  $('#btnResetFilter').on('click', function () {
    if ($('#filterVendor').length) $('#filterVendor').val('').trigger('change');
    $('#filterContractStatus').val('').trigger('change');
    $('#filterApprovalStatus').val('').trigger('change');
    $('#filterStartDate').val('');
    $('#filterEndDate').val('');
    table.ajax.reload();
  });

})(jQuery);
