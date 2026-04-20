(function ($) {
  "use strict";

  const apis = {
    attachments: "/api/attachments",
  };

  let pendingFiles = [];

  function showAttachmentsSection() {
    $("#attachmentsSection").show();
    updatePendingFilesList();
  }

  function hideAttachmentsSection() {
    $("#attachmentsSection").hide();
    $("#attachmentsList").empty();
    pendingFiles = [];
  }

  function getJenisFolder() {
    const jenis = $("#jenis").val();
    if (jenis === "Advanced" || jenis === "Biaya" || jenis === "Realisasi") return jenis;
    return "Other";
  }

  function updatePendingFilesList() {
    const $list = $("#attachmentsList");
    $list.empty();
    if (pendingFiles.length === 0) {
      $list.html('<div class="alert alert-info small">Files will be uploaded after saving the Uang Muka</div>');
      return;
    }
    pendingFiles.forEach((file, idx) => {
      $list.append(`<div class="list-group-item d-flex justify-content-between align-items-center">
        <span>${file.name}</span>
        <button type="button" class="btn btn-sm btn-danger btn-remove-file" data-idx="${idx}"><i class="feather-trash-2"></i></button>
      </div>`);
    });
  }

  $(document).on("click", ".btn-remove-file", function () {
    const idx = $(this).data("idx");
    pendingFiles.splice(idx, 1);
    updatePendingFilesList();
  });

  $("#fileUpload").on("change", function (e) {
    const files = Array.from(e.target.files || []);
    files.forEach(f => {
      if (f.size > 10 * 1024 * 1024) {
        Swal.fire("Error", "File size max 10MB", "error");
        return;
      }
      pendingFiles.push(f);
    });
    e.target.value = "";
    updatePendingFilesList();
  });

  async function uploadPendingFiles(uangMukaId) {
    if (!uangMukaId || pendingFiles.length === 0) return;
    const jenisFolder = getJenisFolder();
    for (const file of pendingFiles) {
      const formData = new FormData();
      formData.append("File", file);
      formData.append("ReferenceId", uangMukaId);
      formData.append("Module", jenisFolder); // Advanced/Biaya/Realisasi
      formData.append("AttachmentTypeId", 1); // Sama dengan Contract
      await authFetch(apis.attachments + "/upload", {
        method: "POST",
        body: formData,
        credentials: "same-origin"
      });
    }
    pendingFiles = [];
    updatePendingFilesList();
  }

  // Expose to global scope for use in transaction-uang-muka.js
  window.uploadPendingFiles = uploadPendingFiles;

  async function loadAttachments(uangMukaId) {
    if (!uangMukaId) return;
    const res = await fetch(`${apis.attachments}?entityId=${uangMukaId}&entityType=UangMuka`);
    const data = await res.json();
    const $list = $("#attachmentsList");
    $list.empty();
    if (!Array.isArray(data) || data.length === 0) {
      $list.html('<div class="alert alert-info small">No attachments found.</div>');
      return;
    }
    data.forEach(att => {
      $list.append(`<div class="list-group-item d-flex justify-content-between align-items-center">
        <a href="${att.url}" target="_blank">${att.fileName}</a>
        <button type="button" class="btn btn-sm btn-danger btn-delete-attachment" data-id="${att.attachmentId}"><i class="feather-trash-2"></i></button>
      </div>`);
    });
  }

  $(document).on("click", ".btn-delete-attachment", async function () {
    const id = $(this).data("id");
    if (!id) return;
    if (!(await Swal.fire({ title: "Delete attachment?", icon: "warning", showCancelButton: true })).isConfirmed) return;
    await fetch(`${apis.attachments}/${id}`, { method: "DELETE", credentials: "same-origin" });
    const uangMukaId = $("#uangMukaId").val();
    await loadAttachments(uangMukaId);
  });

  // Selalu tampilkan attachment section pada modal Add/Edit
  $('#uangMukaModal').on('shown.bs.modal', function () {
    const id = $('#uangMukaId').val();
    showAttachmentsSection();
    if (id) {
      loadAttachments(id);
    } else {
      updatePendingFilesList();
    }
  });

  // After save (submit), upload files if any
  $("#uangMukaForm").on("submit", async function (e) {
    // Wait for main save to finish, then upload files if needed
    setTimeout(async function () {
      const id = $('#uangMukaId').val();
      if (id) await uploadPendingFiles(id);
    }, 500);
  });

})(jQuery);
