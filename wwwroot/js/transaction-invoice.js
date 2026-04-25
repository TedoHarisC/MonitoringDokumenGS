(function ($) {
  "use strict";

  const apis = {
    invoices: "/api/invoices",
    vendors: "/api/vendors?page=1&pageSize=2000",
    progressStatuses: "/api/invoice-progress-statuses?page=1&pageSize=2000",
    attachments: "/api/attachments",
    attachmentTypes: "/api/attachment-types?page=1&pageSize=2000",
    currentUser: "/api/auth/me",
  };

  let table;
  let cachedUserVendors = null;
  let cachedVendors = [];
  let cachedStatuses = [];
  let cachedAttachmentTypes = [];
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
    const modal = bootstrap.Modal.getInstance(el) || new bootstrap.Modal(el);
    modal.show();
  }

  function hideModal(modalId) {
    const el = document.getElementById(modalId);
    if (!el) return;
    const modal = bootstrap.Modal.getInstance(el);
    if (modal) {
      modal.hide();
      // Force remove backdrop if stuck
      setTimeout(() => {
        const backdrops = document.querySelectorAll(".modal-backdrop");
        backdrops.forEach((backdrop) => backdrop.remove());
        document.body.classList.remove("modal-open");
        document.body.style.overflow = "";
        document.body.style.paddingRight = "";
      }, 300);
    }
  }

  async function fetchJson(url, options) {
    // Add credentials to include cookies for authentication
    const fetchOptions = {
      ...options,
      credentials: "same-origin",
    };

    const res = await authFetch(url, fetchOptions);
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

  function formatMoney(value) {
    const n = Number(value);
    if (!Number.isFinite(n)) return "-";
    return n.toLocaleString(undefined, {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    });
  }

  function formatDate(value) {
    if (!value) return "";
    const d = new Date(
      typeof value === "string" && !value.endsWith("Z") ? value + "Z" : value,
    );
    if (isNaN(d.getTime())) return String(value);
    return d.toLocaleString("id-ID");
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

    // Then check cached vendors
    const found = cachedVendors.find((v) =>
      [v.vendorId, v.VendorId]
        .filter(Boolean)
        .some((id) => id.toLowerCase() === String(gid).trim().toLowerCase()),
    );

    if (!found) return null;
    return String(found.vendorName || found.VendorName || "").trim() || null;
  }

  function statusNameById(id) {
    const sid = toInt(id);
    if (!sid) return null;
    const found = cachedStatuses.find(
      (s) => toInt(s.progressStatusId || s.ProgressStatusId) === sid,
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

  function getStatusColor(statusId) {
    // Map status IDs to colors - adjust these based on your status meanings
    const statusColors = {
      1: "secondary", // e.g., Draft
      2: "info", // e.g., Submitted
      3: "warning", // e.g., In Review
      4: "success", // e.g., Approved
      5: "danger", // e.g., Rejected
      6: "primary", // e.g., Completed
    };
    return statusColors[statusId] || "secondary";
  }

  async function loadStatuses() {
    try {
      const result = await fetchJson(apis.progressStatuses);
      cachedStatuses = normalizeToArray(result);
      console.log("Loaded statuses:", cachedStatuses.length, "items");
      return cachedStatuses;
    } catch (err) {
      console.error("Failed to load statuses:", err);
      console.error(
        "This usually means the API requires admin role. User will see status IDs instead of names.",
      );
      return [];
    }
  }

  async function loadCurrentUser() {
    const result = await fetchJson(apis.currentUser);
    currentUserVendor = {
      vendorId: result.vendorId,
      vendorName: result.vendorName,
    };
  }

  async function loadVendors() {
    const result = await fetchJson(apis.vendors);
    cachedVendors = normalizeToArray(result);
    return cachedVendors;
  }

  async function loadAttachmentTypes() {
    const result = await fetchJson(apis.attachmentTypes);
    const types = normalizeToArray(result);
    cachedAttachmentTypes = types.filter((t) => t.appliesTo === "INVOICE");
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

  function initTable() {
    table = $("#invoicesTable").DataTable({
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
        url: apis.invoices,
        dataSrc: function (json) {
          return normalizeToArray(json);
        },
      },
      columns: [
        { data: "invoiceNumber" },
        {
          data: "vendorId",
          render: function (data) {
            const vendorName = vendorNameById(data) || data || "Unknown";
            const colorClass = getVendorColor(data);
            return `<span class="badge bg-${colorClass} bg-soft-${colorClass} text-white" style="font-size: 0.875rem; padding: 0.35rem 0.75rem;">${escapeHtml(vendorName)}</span>`;
          },
        },
        {
          data: "progressStatusId",
          render: function (data) {
            const statusName = statusNameById(data) || data || "Unknown";
            const colorClass = getStatusColor(data);
            return `<span class="fw-bold text-${colorClass}" style="font-size: 0.9rem;">${escapeHtml(statusName)}</span>`;
          },
        },
        {
          data: "invoiceAmount",
          className: "text-end",
          render: function (data) {
            return escapeHtml(formatMoney(data));
          },
        },
        {
          data: "taxAmount",
          className: "text-end",
          render: function (data) {
            return escapeHtml(formatMoney(data));
          },
        },
        {
          data: "invoiceYear",
          className: "text-center",
          render: function (data) {
            return escapeHtml(data || "-");
          },
        },
        {
          data: "invoiceMonth",
          className: "text-center",
          render: function (data) {
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
            const monthNum = toInt(data);
            return escapeHtml(monthNames[monthNum] || data || "-");
          },
        },
        {
          data: "isOnTime",
          className: "text-center",
          render: function (data) {
            if (data === true) {
              return '<span class="badge bg-success d-flex align-items-center justify-content-center"><i class="feather-check-circle me-1"></i>Tepat Waktu</span>';
            } else {
              return '<span class="badge bg-danger d-flex align-items-center justify-content-center"><i class="feather-alert-circle me-1"></i>Terlambat</span>';
            }
          },
        },
        {
          data: "createdAt",
          render: function (data) {
            return escapeHtml(formatDate(data));
          },
        },
        {
          data: null,
          orderable: false,
          searchable: false,
          render: function (data, type, row) {
            const id = row.invoiceId || row.InvoiceId;
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
        searchPlaceholder: "Search invoices... ",
        lengthMenu: "_MENU_ / page",
        info: "Showing _START_ to _END_ of _TOTAL_ invoices",
        infoEmpty: "No invoices found",
        zeroRecords: "No matching invoices",
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
  }

  function clearForm() {
    $("#invoiceId").val("");
    $("#vendorId").val("");
    $("#vendorName").val("");
    $("#progressStatusId").val("2");
    $("#progressStatusSelect").val("2");
    $("#invoiceNumber").val("");
    $("#noSAP").val("");
    $("#invoiceDescription").val("");
    $("#invoiceAmount").val("");
    $("#taxAmount").val("");
    $("#invoiceYear").val("");
    $("#invoiceMonth").val("");
  }

  function populateStatusDropdown() {
    const $select = $("#progressStatusSelect");
    $select.empty();
    $select.append(
      '<option value=""> Pilih Progress Status untuk Invoice ini... </option>',
    );

    cachedStatuses
      .sort((a, b) => a.progressStatusId - b.progressStatusId)
      .forEach((status) => {
        const id = status.progressStatusId || status.ProgressStatusId;
        const name = status.name || status.Name || status.code || status.Code;
        $select.append(`<option value="${id}">${escapeHtml(name)}</option>`);
      });
  }

  function openCreate() {
    portalModalToBody("invoiceModal");
    clearForm();
    $("#invoiceModalLabel").text("Create Invoice");

    // Set vendor from current user
    if (currentUserVendor && currentUserVendor.vendorId) {
      $("#vendorId").val(currentUserVendor.vendorId);
      $("#vendorName").val(currentUserVendor.vendorName || "");
    } else {
      Swal.fire(
        "Warning",
        "No vendor assigned to your account. Please contact administrator.",
        "warning",
      );
      return;
    }

    // Set default progress status to 2
    $("#progressStatusId").val("2");
    $("#progressStatusSelect").val("2");

    // Show/hide progress status section based on user role
    console.log("Is current user admin?", isCurrentUserAdmin());
    if (isCurrentUserAdmin()) {
      populateStatusDropdown();
      $("#progressStatusSection").show();
    } else {
      $("#progressStatusSection").hide();
    }

    // Populate attachment types
    populateAttachmentTypeDropdown();

    $("#attachmentsSection").show();
    $("#attachmentsList").html(
      '<div class="alert alert-info small">Files will be uploaded after saving the invoice</div>',
    );
    pendingFiles = [];
    showModal("invoiceModal");
  }

  async function openEdit(id) {
    // Show loading indicator
    Swal.fire({
      title: "Loading...",
      text: "Please wait",
      allowOutsideClick: false,
      allowEscapeKey: false,
      didOpen: () => {
        Swal.showLoading();
      },
    });

    try {
      portalModalToBody("invoiceModal");
      const data = await fetchJson(`${apis.invoices}/${id}`);

      // Close loading
      Swal.close();

      $("#invoiceModalLabel").text("Edit Invoice");
      $("#invoiceId").val(data.invoiceId);

      // Set vendor (should be user's own vendor)
      const vendorName = vendorNameById(data.vendorId) || "";
      $("#vendorId").val(String(data.vendorId || ""));
      $("#vendorName").val(vendorName);

      $("#progressStatusId").val(String(data.progressStatusId || ""));
      $("#progressStatusSelect").val(String(data.progressStatusId || ""));
      $("#invoiceNumber").val(data.invoiceNumber || "");
      $("#noSAP").val(data.noSAP || "");
      $("#invoiceDescription").val(data.invoiceDescription || "");
      $("#invoiceAmount").val(data.invoiceAmount ?? "");
      $("#taxAmount").val(data.taxAmount ?? "");
      $("#invoiceYear").val(data.invoiceYear ?? "");
      $("#invoiceMonth").val(String(data.invoiceMonth || ""));

      // Show/hide progress status section based on user role
      if (isCurrentUserAdmin()) {
        populateStatusDropdown();
        $("#progressStatusSection").show();
        $("#progressStatusSelect").val(String(data.progressStatusId || ""));
      } else {
        $("#progressStatusSection").hide();
      }

      // Populate attachment types
      populateAttachmentTypeDropdown();

      // Show attachments section and load files
      $("#attachmentsSection").show();
      await loadAttachments(data.invoiceId);

      showModal("invoiceModal");
    } catch (err) {
      Swal.close();
      console.error("Failed to load invoice:", err);
      Swal.fire("Error", "Unable to load invoice", "error");
    }
  }

  async function saveInvoice(e) {
    e.preventDefault();

    const id = String($("#invoiceId").val() || "").trim();
    const uid = currentUserId();
    const vendorId = String($("#vendorId").val() || "").trim();
    const isEdit = !!id;

    // Get progress status - from select if admin, otherwise from hidden field
    const progressStatusId = isCurrentUserAdmin()
      ? toInt($("#progressStatusSelect").val())
      : toInt($("#progressStatusId").val());

    const payload = {
      invoiceId: id || undefined,
      vendorId: vendorId,
      progressStatusId: progressStatusId,
      invoiceNumber: String($("#invoiceNumber").val() || "").trim(),
      noSAP: String($("#noSAP").val() || "").trim() || undefined,
      invoiceDescription: String($("#invoiceDescription").val() || "").trim(),
      invoiceAmount: Number($("#invoiceAmount").val() || 0),
      taxAmount: Number($("#taxAmount").val() || 0),
      invoiceYear: toInt($("#invoiceYear").val()),
      invoiceMonth: toInt($("#invoiceMonth").val()),
      createdByUserId: uid || undefined,
      createdBy: uid || undefined,
      updatedBy: uid || undefined,
    };

    if (!payload.vendorId) {
      return Swal.fire(
        "Validation",
        "Vendor is required. Please ensure your account has a vendor assigned.",
        "warning",
      );
    }
    if (!payload.progressStatusId) {
      return Swal.fire("Validation", "Progress status is required.", "warning");
    }
    if (!payload.invoiceNumber) {
      return Swal.fire("Validation", "Invoice number is required.", "warning");
    }
    if (!payload.invoiceYear || payload.invoiceYear < 2000) {
      return Swal.fire(
        "Validation",
        "Invoice year is required and must be valid.",
        "warning",
      );
    }
    if (
      !payload.invoiceMonth ||
      payload.invoiceMonth < 1 ||
      payload.invoiceMonth > 12
    ) {
      return Swal.fire(
        "Validation",
        "Invoice month is required and must be between 1-12.",
        "warning",
      );
    }

    const method = isEdit ? "PUT" : "POST";
    const url = isEdit ? `${apis.invoices}/${id}` : apis.invoices;

    // Show loading
    Swal.fire({
      title: "Saving...",
      text: "Please wait",
      allowOutsideClick: false,
      allowEscapeKey: false,
      didOpen: () => {
        Swal.showLoading();
      },
    });

    try {
      const result = await fetchJson(url, {
        method,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });

      console.log("Invoice saved successfully. Result:", result);

      // Upload pending files if any (for both create and edit)
      const invoiceId = (result && result.invoiceId) || id;
      console.log("Invoice ID for upload:", invoiceId);
      console.log("Pending files count:", pendingFiles.length);
      console.log("Pending files:", pendingFiles);

      if (pendingFiles.length > 0 && invoiceId) {
        console.log("Starting upload of pending files for invoice:", invoiceId);
        await uploadPendingFiles(invoiceId);
        console.log("All pending files uploaded successfully");
      } else {
        if (pendingFiles.length === 0) {
          console.log("No pending files to upload");
        }
        if (!invoiceId) {
          console.error("ERROR: Invoice ID is missing! Cannot upload files.");
        }
      }

      hideModal("invoiceModal");
      if (table) table.ajax.reload(null, false);

      // Single success message
      Swal.fire({
        icon: "success",
        title: "Saved",
        text: "Invoice saved successfully",
        timer: 1500,
        showConfirmButton: false,
      });
    } catch (err) {
      const message =
        (err && (err.message || err.title)) || "Unable to save invoice.";
      Swal.fire("Error", message, "error");
    }
  }

  async function deleteInvoice(id) {
    const res = await Swal.fire({
      title: "Delete invoice?",
      text: "This action cannot be undone.",
      icon: "warning",
      showCancelButton: true,
      confirmButtonText: "Delete",
      cancelButtonText: "Cancel",
    });

    if (!res.isConfirmed) return;

    try {
      console.log("Deleting invoice:", id);
      const response = await authFetch(`${apis.invoices}/${id}`, {
        method: "DELETE",
        credentials: "same-origin",
      });

      console.log("Delete response status:", response.status);

      // Handle different response types
      if (response.status === 204) {
        // No content - successful delete
        if (table) table.ajax.reload(null, false);
        Swal.fire("Deleted", "Invoice deleted", "success");
        return;
      }

      if (response.status === 403 || response.status === 401) {
        Swal.fire(
          "Access Denied",
          "You do not have permission to delete invoices",
          "error",
        );
        return;
      }

      if (!response.ok) {
        const contentType = response.headers.get("content-type");
        let errorMessage = "Unable to delete invoice.";

        if (contentType && contentType.includes("application/json")) {
          const errorData = await response.json();
          errorMessage = errorData.message || errorMessage;
        } else {
          // HTML response (error page)
          errorMessage = `Server error (${response.status})`;
        }

        throw new Error(errorMessage);
      }

      // Try to parse JSON if available
      const data = await response.json().catch(() => null);

      if (table) table.ajax.reload(null, false);
      Swal.fire("Deleted", "Invoice deleted", "success");
    } catch (err) {
      console.error("Delete error:", err);
      const message = err.message || "Unable to delete invoice.";
      Swal.fire("Error", message, "error");
    }
  }

  async function loadAttachments(invoiceId) {
    try {
      const attachments = await fetchJson(
        `${apis.attachments}/by-reference/${invoiceId}`,
      );
      const list = $("#attachmentsList");
      list.empty();

      if (!attachments || attachments.length === 0) {
        list.html(
          '<div class="text-muted text-center py-3">No attachments yet</div>',
        );
        return;
      }

      attachments.forEach((att) => {
        const sizeKB = ((att.fileSize || 0) / 1024).toFixed(1);
        const typeName = att.attachmentTypeName || "";

        const item = $(`
                    <div class="list-group-item d-flex justify-content-between align-items-center">
                        <div class="flex-grow-1" style="cursor: pointer;">
                            <a href="#" class="text-decoration-none text-dark d-flex align-items-center btn-download-attachment" data-id="${att.attachmentId}" data-filename="${escapeHtml(att.fileName)}">
                                <i class="feather-file me-2"></i>
                                <span class="text-primary">${escapeHtml(att.fileName)}</span>
                                ${typeName ? `<span class="badge bg-info ms-2">${escapeHtml(typeName)}</span>` : ""}
                                <small class="text-muted ms-2">(${sizeKB} KB)</small>
                                <i class="feather-download ms-2 text-muted" style="font-size: 14px;"></i>
                            </a>
                        </div>
                        <button type="button" class="btn btn-sm btn-light-danger btn-delete-attachment ms-2" data-id="${att.attachmentId}">
                            <i class="feather-trash-2"></i>
                        </button>
                    </div>
                `);
        list.append(item);
      });
    } catch (err) {
      console.error("Failed to load attachments:", err);
    }
  }

  async function uploadFile(file, invoiceId, typeId = null) {
    // If no invoiceId yet (create mode), add to pending list
    if (!invoiceId) {
      console.log("No invoiceId yet, adding to pending files:", file.name);
      pendingFiles.push(file);
      updatePendingFilesList();
      $("#fileUpload").val(""); // Clear input for next file
      return;
    }

    console.log(
      "Uploading file:",
      file.name,
      "for invoice:",
      invoiceId,
      "with typeId:",
      typeId,
    );

    const formData = new FormData();
    formData.append("file", file);
    formData.append("module", "Invoices");
    formData.append("attachmentTypeId", typeId || "1");
    formData.append("referenceId", invoiceId);

    // Log actual FormData contents
    console.log("FormData contents:");
    for (let [key, value] of formData.entries()) {
      console.log(`  ${key}:`, value);
    }

    try {
      console.log("Sending upload request to:", `${apis.attachments}/upload`);

      // Don't use authFetch - use manual fetch with explicit headers
      const token = localStorage.getItem("mdgs_token");
      const headers = {};
      if (token) {
        headers["Authorization"] = "Bearer " + token;
      }
      // DO NOT set Content-Type - let browser set it automatically for FormData

      const res = await fetch(`${apis.attachments}/upload`, {
        method: "POST",
        headers: headers,
        body: formData,
        credentials: "same-origin",
      });

      console.log("Upload response status:", res.status);

      if (!res.ok) {
        const err = await res
          .json()
          .catch(() => ({ message: "Upload failed" }));
        console.error("Upload failed with error:", err);
        throw err;
      }

      const result = await res.json();
      console.log("Upload successful, result:", result);

      await loadAttachments(invoiceId);
      $("#fileUpload").val(""); // Clear input for next file

      console.log("File upload completed successfully");
    } catch (err) {
      console.error("Upload error:", err);
      const message = (err && (err.message || err.title)) || "Upload failed.";
      Swal.fire("Error", message, "error");
    }
  }

  function updatePendingFilesList() {
    const $list = $("#attachmentsList");
    $list.empty();

    if (pendingFiles.length === 0) {
      $list.html(
        '<div class="alert alert-info small">Files will be uploaded after saving the invoice</div>',
      );
      return;
    }

    $list.append(
      '<div class="alert alert-info small mb-2">Files to upload after save:</div>',
    );
    pendingFiles.forEach((pendingFile, index) => {
      // Handle both old (file object) and new ({file, typeId, typeName}) structure
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

  async function uploadPendingFiles(invoiceId) {
    if (pendingFiles.length === 0) return;

    console.log(
      `Uploading ${pendingFiles.length} pending files for invoice:`,
      invoiceId,
    );

    let uploadedCount = 0;
    let failedCount = 0;

    for (const pendingFile of pendingFiles) {
      try {
        // Handle both old (file object) and new ({file, typeId, typeName}) structure
        const file = pendingFile.file || pendingFile;
        const typeId = pendingFile.typeId || null;

        console.log(
          "Processing pending file:",
          file.name,
          "with typeId:",
          typeId,
        );
        await uploadFile(file, invoiceId, typeId);
        uploadedCount++;
        console.log(
          `File ${file.name} uploaded successfully (${uploadedCount}/${pendingFiles.length})`,
        );
      } catch (err) {
        failedCount++;
        console.error(
          "Failed to upload file:",
          pendingFile.file?.name || pendingFile.name,
          "Error:",
          err,
        );
        // Continue uploading other files even if one fails
      }
    }

    pendingFiles = [];
    console.log(
      `Upload complete: ${uploadedCount} succeeded, ${failedCount} failed`,
    );

    if (failedCount > 0) {
      Swal.fire({
        icon: "warning",
        title: "Some Files Failed",
        text: `${uploadedCount} files uploaded successfully, but ${failedCount} files failed to upload.`,
      });
    }
  }

  async function deleteAttachment(attachmentId, invoiceId) {
    const res = await Swal.fire({
      title: "Delete attachment?",
      text: "This action cannot be undone.",
      icon: "warning",
      showCancelButton: true,
      confirmButtonText: "Delete",
      cancelButtonText: "Cancel",
    });

    if (!res.isConfirmed) return;

    try {
      await fetchJson(`${apis.attachments}/${attachmentId}`, {
        method: "DELETE",
      });
      await loadAttachments(invoiceId);
      Swal.fire("Deleted", "Attachment deleted", "success");
    } catch (err) {
      const message = (err && (err.message || err.title)) || "Delete failed.";
      Swal.fire("Error", message, "error");
    }
  }

    $(async function () {
      portalModalToBody("invoiceModal");

      try {
        await Promise.all([
          loadCurrentUser(),
          loadStatuses(),
          loadVendors(),
          loadAttachmentTypes(),  
        ]);
      } catch (err) {
        console.error("Initialization error:", err);
      }

      initTable();

      $("#btnCreateInvoice").on("click", openCreate);

      $("#invoicesTable").on("click", ".btn-edit", function () {
        const id = $(this).data("id");
        openEdit(id);
      });

      $("#invoicesTable").on("click", ".btn-delete", function () {
        const id = $(this).data("id");
        deleteInvoice(id);
      });

      $("#invoiceForm").on("submit", saveInvoice);

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

        // Validate file size (max 10MB)
        const maxSize = 10 * 1024 * 1024;
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

        console.log("Adding file to pending:", pendingFile);
        pendingFiles.push(pendingFile);
        updatePendingFilesList();

        // Clear inputs
        fileInput.value = "";
        $("#attachmentTypeSelect").val("");

        Swal.fire({
          icon: "success",
          title: "File Added",
          text: `${file.name} will be uploaded after saving the invoice.`,
          timer: 2000,
          showConfirmButton: false,
        });
      });

      // File upload handler (kept for backward compatibility if needed)
      $("#fileUpload").on("change", function (e) {
        const file = e.target.files[0];
        if (!file) return;

        const invoiceId = $("#invoiceId").val();
        uploadFile(file, invoiceId);
      });

      // Delete attachment handler
      $(document).on("click", ".btn-delete-attachment", function () {
        const attachmentId = $(this).data("id");
        const invoiceId = $("#invoiceId").val();
        deleteAttachment(attachmentId, invoiceId);
      });

      // Download attachment handler (uses auth token)
      $(document).on("click", ".btn-download-attachment", function (e) {
        e.preventDefault();
        const attachmentId = $(this).data("id");
        const token = localStorage.getItem("mdgs_token");

        // Open direct download URL with token in query string (works over HTTP)
        const downloadUrl = `${apis.attachments}/file/${attachmentId}?token=${encodeURIComponent(token)}`;
        window.open(downloadUrl, "_blank");
      });

      // Remove pending file handler
      $(document).on("click", ".btn-remove-pending", function () {
        const index = $(this).data("index");
        pendingFiles.splice(index, 1);
        updatePendingFilesList();
      });

      // ===================== INVOICE DETAIL HANDLER =====================
      // Add detail button to table actions (patch DataTable render)
      // Or use delegated event for detail button
      $("#invoicesTable").on("click", ".btn-detail", function () {
        const id = $(this).data("id");
        openInvoiceDetail(id);
      });

      // Patch DataTable actions column to add Detail button
      // (Monkey patch, since DataTable is already initialized)
      if (table) {
        const oldRender = table.column(-1).settings()[0].aoColumns[9].mRender;
        table.column(-1).settings()[0].aoColumns[9].mRender = function (data, type, row) {
          const id = row.invoiceId || row.InvoiceId;
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
        };
        table.draw(false);
      }

      // Handler for detail modal
      async function openInvoiceDetail(id) {
        portalModalToBody("invoiceDetailModal");
        $("#invoiceDetailContent").html('<div class="text-center py-4"><div class="spinner-border"></div></div>');
        showModal("invoiceDetailModal");

        try {
          // Fetch invoice data
          const data = await fetchJson(`${apis.invoices}/${id}`);
          // Fetch attachments
          const attachments = await fetchJson(`${apis.attachments}/by-reference/${id}`);

          // Render detail content in grid (2 columns)
          let html = `<div class="row g-3">
            <div class="col-md-6">
              <div class="fw-bold mb-1">Invoice Number:</div>
              <div>${escapeHtml(data.invoiceNumber)}</div>
            </div>
            <div class="col-md-6">
              <div class="fw-bold mb-1">Vendor:</div>
              <div>${escapeHtml(vendorNameById(data.vendorId) || data.vendorId)}</div>
            </div>
            <div class="col-md-6">
              <div class="fw-bold mb-1">Status:</div>
              <div>${escapeHtml(statusNameById(data.progressStatusId) || data.progressStatusId)}</div>
            </div>
            <div class="col-md-6">
              <div class="fw-bold mb-1">Invoice Amount:</div>
              <div>${escapeHtml(formatMoney(data.invoiceAmount))}</div>
            </div>
            <div class="col-md-6">
              <div class="fw-bold mb-1">Tax Amount:</div>
              <div>${escapeHtml(formatMoney(data.taxAmount))}</div>
            </div>
            <div class="col-md-6">
              <div class="fw-bold mb-1">Year / Month:</div>
              <div>${escapeHtml(data.invoiceYear)} / ${escapeHtml(data.invoiceMonth)}</div>
            </div>
            <div class="col-md-12">
              <div class="fw-bold mb-1">Description:</div>
              <div>${escapeHtml(data.invoiceDescription)}</div>
            </div>
            <div class="col-md-6">
              <div class="fw-bold mb-1">Created At:</div>
              <div>${escapeHtml(formatDate(data.createdAt))}</div>
            </div>
            <div class="col-md-6">
              <div class="fw-bold mb-1">On-Time Status:</div>
              <div>${data.isOnTime ? '<span class="badge bg-success">Tepat Waktu</span>' : '<span class="badge bg-danger">Terlambat</span>'}</div>
            </div>
          </div>`;

          // Attachments
          html += `<div class="mt-4 mb-3">
            <div class="fw-bold mb-2">Attachments:</div>`;
          if (attachments && attachments.length > 0) {
            html += '<ul class="list-group">';
            attachments.forEach(att => {
              const sizeKB = ((att.fileSize || 0) / 1024).toFixed(1);
              html += `<li class="list-group-item d-flex justify-content-between align-items-center">
                <div class="d-flex align-items-center">
                  <i class="feather-file me-2"></i>
                  <span>${escapeHtml(att.fileName)}</span>
                  <span class="badge bg-info ms-2">${escapeHtml(att.attachmentTypeName || "")}</span>
                  <small class="text-muted ms-2">(${sizeKB} KB)</small>
                </div>
                <a href="${apis.attachments}/download/${att.attachmentId}" class="btn btn-sm btn-outline-primary ms-2" target="_blank">
                  <i class="feather-download"></i> Download
                </a>
              </li>`;
            });
            html += '</ul>';
          } else {
            html += '<div class="text-muted">Belum ada dokumen yang diupload oleh GS</div>';
          }
          html += '</div>';

          // Timeline History Section
          html += `<div class="mt-5">
            <h6 class="text-primary mb-3"><i class="feather-clock me-1"></i> History Perubahan Status</h6>
            <div id="invoiceHistoryTimeline" class="timeline-container"></div>
          </div>`;
          $("#invoiceDetailContent").html(html);
          // Load timeline after detail
          if (id) window.renderInvoiceDetailHistory && window.renderInvoiceDetailHistory(id);
        } catch (err) {
          $("#invoiceDetailContent").html('<div class="text-danger">Gagal memuat detail invoice.</div>');
        }
      }
      // ===================== END INVOICE DETAIL HANDLER =====================
    });
})(jQuery);

// Timeline History Loader for Invoice Detail
  function loadInvoiceHistory(invoiceId) {
    const $timeline = $("#invoiceHistoryTimeline");
    $timeline.html('<div class="text-muted">Loading history...</div>');
    $.get(`/api/invoices/${invoiceId}/history`)
      .done(function (data) {
        if (!data || !Array.isArray(data) || data.length === 0) {
          $timeline.html('<div class="text-muted">Tidak ada history perubahan.</div>');
          return;
        }
        // Sort by CreatedAt ascending
        data.sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt));
        let html = '<div class="timeline">';
        data.forEach(function (item, idx) {
          let status = "";
          let user = item.userName || (item.userId ? `User: ${item.userId}` : "-");
          let tgl = new Date(item.createdAt).toLocaleString();
          try {
            if (item.newData) {
              const newData = typeof item.newData === 'string' ? JSON.parse(item.newData) : item.newData;
              status = newData.status || newData.Status || newData.progressStatusId || newData.ProgressStatusId || "-";
            }
          } catch {}
          html += `
            <div class="timeline-item mb-4">
              <div class="d-flex align-items-center gap-2 mb-1">
                <span class="badge bg-primary">${status}</span>
                <span class="fw-bold">${user}</span>
                <span class="text-muted small">${tgl}</span>
              </div>
              <div class="text-muted">${item.entityName || item.action || ""}</div>
            </div>`;
        });
        html += '</div>';
        $timeline.html(html);
      })
      .fail(function (xhr) {
        if (xhr.status === 404 && xhr.responseJSON && xhr.responseJSON.message && xhr.responseJSON.message.includes('No history')) {
          $timeline.html('<div class="text-muted">Tidak ada history perubahan.</div>');
        } else {
          $timeline.html('<div class="text-danger">Gagal memuat history perubahan.</div>');
        }
      });
  }
  window.renderInvoiceDetailHistory = function(invoiceId) {
    loadInvoiceHistory(invoiceId);
  };
