$(document).ready(function () {
  function isCurrentUserAdmin() {
    const fromWindow = window.currentUserIsAdmin;
    if (typeof fromWindow === "boolean") return fromWindow;
    if (typeof fromWindow === "string") {
      const normalized = fromWindow.trim().toLowerCase();
      if (normalized === "true") return true;
    }

    const el = document.getElementById("currentUserIsAdmin");
    const normalized = el
      ? String(el.value || "")
          .trim()
          .toLowerCase()
      : "false";
    return normalized === "true";
  }

  // Function to format file size (used in detail modal)
  function formatFileSize(bytes) {
    if (bytes === 0) return "0 B";
    const k = 1024;
    const sizes = ["B", "KB", "MB", "GB", "TB"];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
  }

  // DataTable init
  var table = $("#templateTable").DataTable({
    ajax: {
      url: "/api/template-files",
      dataSrc: function (json) {
        const rows = Array.isArray(json)
          ? json
          : json?.items || json?.data || [];
        // Jika bukan admin, filter hanya permission 'all' dan 'user'
        if (isCurrentUserAdmin()) return rows;
        return rows.filter(
          (x) => x.permission === "all" || x.permission === "user",
        );
      },
    },
    columns: [
      { data: null, render: (data, type, row, meta) => meta.row + 1 },
      {
        data: "title",
        render: (data, type, row) =>
          `<a href="#" class="template-detail-link" data-id="${row.id}">${data}</a>`,
      },
      { data: "permission" },
      {
        data: "fileName",
        render: (data, type, row) =>
          data
            ? `<a href="#" class="template-detail-link" data-id="${row.id}">${data}</a>`
            : "-",
      },
      {
        data: "createdAt",
        render: (data) => (data ? new Date(data).toLocaleString() : "-"),
      },
      {
        data: null,
        className: "dt-actions text-end",
        orderable: false,
        render: function (data, type, row) {
          return `
                        <button class="btn btn-sm btn-warning btn-edit" data-id="${row.id}"><i class="feather-edit"></i></button>
                        <button class="btn btn-sm btn-danger btn-delete" data-id="${row.id}"><i class="feather-trash-2"></i></button>
                    `;
        },
      },
    ],
  });

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

  // Track last trigger for modal detail
  let lastDetailTrigger = null;
  function hideModal(modalId, options = {}) {
    const el = document.getElementById(modalId);
    if (!el) return;
    const modal = bootstrap.Modal.getInstance(el);
    // Perbaikan aksesibilitas: pindahkan fokus sebelum hide jika modal detail
    if (modalId === "templateDetailModal" && lastDetailTrigger) {
      lastDetailTrigger.focus();
      lastDetailTrigger = null;
    } else if (options.restoreFocus && options.triggerEl) {
      options.triggerEl.focus();
    }
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

  // Open modal for add
  $("#btnAddBudget").on("click", function () {
    clearModal();
    $("#templateModalLabel").text("Add Template");
    $("#currentFileInfo").html("");
    $("#templateFile").prop("required", true);
    portalModalToBody("templateModal");
    showModal("templateModal");
  });

  // Save (add or edit)
  $("#templateForm").on("submit", function (e) {
    e.preventDefault();
    var id = $("#templateId").val();
    var isEdit = !!id;
    var form = document.getElementById("templateForm");
    var formData = new FormData(form);
    // Map field names to API
    formData.set("Title", $("#judul").val());
    formData.set("Permission", $("#permission").val());
    formData.set("Module", "TemplateFiles");
    var fileInput = document.getElementById("templateFile");
    var fileObj =
      fileInput && fileInput.files && fileInput.files[0]
        ? fileInput.files[0]
        : null;
    if (!isEdit) {
      // Add: multipart/form-data (upload)
      if (fileObj) {
        formData.set("FileSize", fileObj.size);
      }
      $.ajax({
        url: "/api/template-files/upload",
        method: "POST",
        data: formData,
        processData: false,
        contentType: false,
        success: function () {
          hideModal("templateModal");
          table.ajax.reload();
        },
        error: function (xhr) {
          alert(xhr.responseJSON?.message || "Failed to upload template");
        },
      });
    } else {
      // Edit: jika user pilih file baru, upload file baru via endpoint upload
      if (fileObj) {
        formData.set("FileSize", fileObj.size);
        formData.set("Id", id);
        $.ajax({
          url: "/api/template-files/upload",
          method: "POST",
          data: formData,
          processData: false,
          contentType: false,
          success: function () {
            hideModal("templateModal");
            table.ajax.reload();
          },
          error: function (xhr) {
            alert(xhr.responseJSON?.message || "Failed to update template");
          },
        });
      } else {
        // Edit tanpa file baru: PUT JSON
        var data = {
          id: id,
          title: $("#judul").val(),
          permission: $("#permission").val(),
          fileName: $("#templateFile").data("filename") || "",
          filePath: $("#templateFile").data("filepath") || "",
          fileSize: $("#templateFile").data("filesize") || 0,
        };
        $.ajax({
          url: `/api/template-files/${id}`,
          method: "PUT",
          contentType: "application/json",
          data: JSON.stringify(data),
          success: function () {
            hideModal("templateModal");
            table.ajax.reload();
          },
          error: function (xhr) {
            alert(xhr.responseJSON?.message || "Failed to update template");
          },
        });
      }
    }
  });

  // Edit
  $("#templateTable").on("click", ".btn-edit", function () {
    var id = $(this).data("id");
    $.get(`/api/template-files/${id}`, function (data) {
      clearModal();
      $("#templateId").val(data.id);
      $("#judul").val(data.title);
      $("#permission").val(data.permission);
      // File info (not editable)
      $("#templateFile").data("filename", data.fileName);
      $("#templateFile").data("filepath", data.filePath);
      $("#templateFile").val("");
      $("#templateFile").prop("required", false);
      if (data.fileName && data.filePath) {
        $("#currentFileInfo").html(
          `<div class="alert alert-info py-2 px-3 mb-0">File saat ini: <a href="/api/template-files/download/${data.id}" target="_blank">${data.fileName}</a></div>`,
        );
      } else {
        $("#currentFileInfo").html(
          '<div class="text-muted small">Belum ada file</div>',
        );
      }
      $("#templateModalLabel").text("Edit Template");
      portalModalToBody("templateModal");
      showModal("templateModal");
    });
  });

  // Delete
  $("#templateTable").on("click", ".btn-delete", function () {
    if (!confirm("Delete this template?")) return;
    var id = $(this).data("id");
    $.ajax({
      url: `/api/template-files/${id}`,
      method: "DELETE",
      success: function () {
        table.ajax.reload();
      },
      error: function (xhr) {
        alert(xhr.responseJSON?.message || "Failed to delete template");
      },
    });
  });

  function clearModal() {
    $("#templateId").val("");
    $("#judul").val("");
    $("#permission").val("all");
    $("#templateFile").val("");
    $("#templateFile").removeData("filename").removeData("filepath");
    $("#currentFileInfo").html("");
    $("#templateFile").prop("required", true);
  }

  // Show detail modal when clicking file link or judul
  $(document).on("click", ".template-detail-link", function (e) {
    e.preventDefault();
    var id = $(this).data("id");
    lastDetailTrigger = this; // Simpan elemen trigger
    $.get("/api/template-files/" + id, function (data) {
      let permissionText = "";
      switch (data.permission) {
        case "all":
          permissionText = "Semua User";
          break;
        case "admin":
          permissionText = "Admin (GS)";
          break;
        case "user":
          permissionText = "User (Vendor)";
          break;
        default:
          permissionText = data.permission;
      }

      var html = `
                <dl class="row">
                <dt class="col-sm-4">Judul</dt><dd class="col-sm-8">${data.title}</dd>
                <dt class="col-sm-4">Permission</dt><dd class="col-sm-8">${permissionText}</dd>
                <dt class="col-sm-4">Created At</dt><dd class="col-sm-8">${data.createdAt ? new Date(data.createdAt).toLocaleString() : "-"}</dd>
                <dt class="col-sm-4">File Size</dt><dd class="col-sm-8 fw-semibold">${data.fileSize ? formatFileSize(data.fileSize) : "-"}</dd>
                <dt class="col-sm-4">File</dt><dd class="col-sm-8">`;
      if (data.fileName && data.filePath) {
        html += `<a href="/api/template-files/download/${data.id}" class="btn btn-sm btn-primary py-2"><i class="feather-download"></i>&nbsp; Download</a>`;
      } else {
        html += "-";
      }
      html += `<dt class="col-sm-4">File Name</dt><dd class="col-sm-8 fw-semibold">${data.fileName}</dd>`;
      html += "</dd></dl>";
      $("#templateDetailContent").html(html);
      portalModalToBody("templateDetailModal");
      showModal("templateDetailModal");
    });
  });

  // Handler untuk close modal detail (aksesibilitas) - tidak diperlukan lagi, sudah dipindahkan ke hideModal
});
