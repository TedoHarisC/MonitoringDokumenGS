$(function () {
    const apiBase = "/api/uang-muka";
    let uangMukaTable;
    let modalInstance = null;

    // ─── KEY FIX: Pindahkan modal langsung ke document.body ──────────────────
    // Ini yang membuat modal Invoice tidak kena overlay — salin teknik yang sama
    function portalModalToBody(modalId) {
        const el = document.getElementById(modalId);
        if (!el) return;
        if (el.parentElement !== document.body) {
            document.body.appendChild(el);
        }
    }

    // ─── Helper: show modal ───────────────────────────────────────────────────
    function showModal() {
        const el = document.getElementById('uangMukaModal');
        const modal = bootstrap.Modal.getInstance(el) || new bootstrap.Modal(el, {
            backdrop: 'static',
            keyboard: false
        });
        modal.show();
    }

    // ─── Helper: hide modal + force cleanup backdrop ──────────────────────────
    function hideModal() {
        const el = document.getElementById('uangMukaModal');
        const modal = bootstrap.Modal.getInstance(el);
        if (modal) {
            modal.hide();
            setTimeout(() => {
                document.querySelectorAll('.modal-backdrop').forEach(el => el.remove());
                document.body.classList.remove('modal-open');
                document.body.style.overflow = '';
                document.body.style.paddingRight = '';
            }, 300);
        }
    }

    // ─── Load Budget Codes + Select2 ─────────────────────────────────────────
    function loadBudgetCodes() {
        return $.getJSON("/api/budget-codes?page=1&pageSize=2000").then(function (res) {
            const items = Array.isArray(res) ? res : (res.items || res.data || []);
            const $sel = $("#budgetCodeId");
            $sel.empty().append('<option value="">-- Pilih Budget Code --</option>');
            items.forEach(function (b) {
                $sel.append(`<option value="${b.budgetCodeId}">${b.code} - ${b.description}</option>`);
            });
            // Inisialisasi Select2 setelah isi option
            $sel.select2({
                theme: "bootstrap-5",
                placeholder: "-- Pilih Budget Code --",
                allowClear: true,
                width: "100%",
                dropdownParent: $("#uangMukaModal")
            });
        });
    }

    // ─── Load COA Text (Vendor Categories) + Select2 ─────────────────────────
    function loadCoaTexts() {
        return $.getJSON("/api/vendor-categories?page=1&pageSize=2000").then(function (res) {
            const items = Array.isArray(res) ? res : (res.items || res.data || []);
            const $sel = $("#coaTextId");
            $sel.empty().append('<option value="">-- Pilih COA Text --</option>');
            items.forEach(function (v) {
                $sel.append(`<option value="${v.vendorCategoryId}">${v.parentBudgetCodeLabel || "-"} - ${v.name}</option>`);
            });
            $sel.select2({
                theme: "bootstrap-5",
                placeholder: "-- Pilih COA Text --",
                allowClear: true,
                width: "100%",
                dropdownParent: $("#uangMukaModal")
            });
        });
    }
    function loadStatusOptions(jenis) {
        const url = jenis === "Advanced"
            ? "/api/advanced-statuses?page=1&pageSize=2000"
            : "/api/biaya-realisasi-statuses?page=1&pageSize=2000";

        return $.getJSON(url).then(function (res) {
            const items = Array.isArray(res) ? res : (res.items || res.data || []);
            const $sel = $("#statusId");
            $sel.empty().append('<option value="">-- Pilih Status --</option>');
            items.forEach(function (s) {
                const id = jenis === "Advanced" ? s.advancedStatusId : s.biayaRealisasiStatusId;
                $sel.append(`<option value="${id}">${s.code} - ${s.name}</option>`);
            });
        });
    }

    // ─── Reload DataTable ─────────────────────────────────────────────────────
    function reloadTable() {
        uangMukaTable.ajax.reload(null, false);
    }

    // ─── Init: Portal modal ke body saat halaman load ─────────────────────────
    portalModalToBody('uangMukaModal');

    // ─── Init DataTable ───────────────────────────────────────────────────────
    uangMukaTable = $("#unagMukasTable").DataTable({
        ajax: {
            url: apiBase,
            dataSrc: function (json) {
                //console.log("Raw API Response:", json);
                return Array.isArray(json) ? json : (json.items || json.data || []);
            }
        },
        columns: [
            { data: "atasNama" },
            { data: "jenis" },
            {
                data: "amount",
                render: $.fn.dataTable.render.number(',', '.', 2, '')
            },
            { data: "budgetCode.description", defaultContent: "-" },
            { data: "coaText.name", defaultContent: "-" },
            {
                data: "startDate",
                render: function (d) { return d ? d.split("T")[0] : "-"; }
            },
            {
                data: "endDate",
                render: function (d) { return d ? d.split("T")[0] : "-"; }
            },
            { data: "status", defaultContent: "-" },
            {
                data: null,
                orderable: false,
                searchable: false,
                className: "dt-actions text-center",
                render: function (data, type, row) {
                    return `
                        <button class="btn btn-sm btn-light-brand btn-edit" data-id="${row.uangMukaId}">
                            <i class="feather-edit-2 me-1"></i>Edit
                        </button>
                        <button class="btn btn-sm btn-light-danger btn-delete" data-id="${row.uangMukaId}">
                            <i class="feather-trash-2 me-1"></i>Delete
                        </button>`;
                }
            }
        ],
        pageLength: 10,
        lengthMenu: [10, 20, 50, 100, 200],
        pagingType: "simple_numbers",
        scrollX: false,
        autoWidth: false,
        language: {
            search: "",
            searchPlaceholder: "Search...",
            lengthMenu: "_MENU_ / page",
            info: "Showing _START_ to _END_ of _TOTAL_ items",
            infoEmpty: "No items found",
            zeroRecords: "No matching items",
            paginate: {
                previous: '<i class="feather-chevron-left"></i>',
                next: '<i class="feather-chevron-right"></i>'
            }
        }
    });

    // ─── Event: modal hidden → force cleanup backdrop ─────────────────────────
    document.getElementById('uangMukaModal').addEventListener('hidden.bs.modal', function () {
        setTimeout(() => {
            document.querySelectorAll('.modal-backdrop').forEach(el => el.remove());
            document.body.classList.remove('modal-open');
            document.body.style.overflow = '';
            document.body.style.paddingRight = '';
        }, 300);
    });

    // ─── Event: Tombol Add New Data ───────────────────────────────────────────
    $("#btnCreateContract").on("click", function () {
        portalModalToBody('uangMukaModal');
        $("#uangMukaForm")[0].reset();
        $("#uangMukaId").val("");
        $("#uangMukaModalLabel").text("Create Uang Muka");

        Promise.all([
            loadBudgetCodes(),
            loadCoaTexts(),
            loadStatusOptions("Advanced")
        ]).then(function () {
            $("#jenis").val("");
            $("#statusId").val("");
            $("#budgetCodeId").val("").trigger("change");
            $("#coaTextId").val("").trigger("change");
            showModal();
        });
    });

    // ─── Event: Jenis berubah → reload status ────────────────────────────────
    $("#jenis").on("change", function () {
        const val = $(this).val();
        if (val) loadStatusOptions(val);
    });

    // ─── Event: Tombol Edit ───────────────────────────────────────────────────
    $("#unagMukasTable").on("click", ".btn-edit", function () {
        const id = $(this).data("id");
        $.get(`${apiBase}/${id}`).done(function (data) {
            portalModalToBody('uangMukaModal');
            $("#uangMukaId").val(data.uangMukaId);
            $("#jenis").val(data.jenis);
            $("#atasNama").val(data.atasNama);
            $("#amount").val(data.amount);
            $("#startDate").val(data.startDate ? data.startDate.split("T")[0] : "");
            $("#endDate").val(data.endDate ? data.endDate.split("T")[0] : "");
            $("#noSAP").val(data.noSAP || "");
            $("#deskripsi").val(data.deskripsi || "");
            $("#uangMukaModalLabel").text("Edit Uang Muka");

            Promise.all([
                loadBudgetCodes(),
                loadCoaTexts(),
                loadStatusOptions(data.jenis)
            ]).then(function () {
                $("#budgetCodeId").val(data.budgetCodeId).trigger("change");
                $("#coaTextId").val(data.coaTextId).trigger("change");
                $("#statusId").val(data.statusId);
                showModal();
            });
        }).fail(function () {
            Swal.fire("Error", "Gagal memuat data Uang Muka.", "error");
        });
    });

    // ─── Event: Tombol Delete ─────────────────────────────────────────────────
    $("#unagMukasTable").on("click", ".btn-delete", function () {
        const id = $(this).data("id");
        Swal.fire({
            title: "Hapus Uang Muka?",
            text: "Tindakan ini tidak dapat dibatalkan.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Delete",
            cancelButtonText: "Cancel"
        }).then(function (result) {
            if (!result.isConfirmed) return;
            $.ajax({
                url: `${apiBase}/${id}`,
                method: "DELETE"
            }).done(function () {
                reloadTable();
                Swal.fire("Deleted", "Uang Muka berhasil dihapus.", "success");
            }).fail(function (xhr) {
                Swal.fire("Error", xhr.responseJSON?.message || "Gagal menghapus.", "error");
            });
        });
    });

    // ─── Event: Form Submit (Create / Update) ─────────────────────────────────
    $("#uangMukaForm").on("submit", function (e) {
        e.preventDefault();

        const id = $("#uangMukaId").val();
        const payload = {
            uangMukaId: id || undefined,
            jenis: $("#jenis").val(),
            budgetCodeId: $("#budgetCodeId").val(),
            coaTextId: $("#coaTextId").val(),
            atasNama: $("#atasNama").val(),
            amount: parseFloat($("#amount").val()),
            startDate: $("#startDate").val(),
            endDate: $("#endDate").val(),
            statusId: $("#statusId").val(),
            noSAP: $("#noSAP").val(),
            deskripsi: $("#deskripsi").val()
        };

        const method = id ? "PUT" : "POST";
        const url = id ? `${apiBase}/${id}` : apiBase;

        $.ajax({
            url: url,
            method: method,
            contentType: "application/json",
            data: JSON.stringify(payload)
        }).done(function () {
            hideModal();
            reloadTable();
            Swal.fire("Saved", "Uang Muka berhasil disimpan.", "success");
        }).fail(function (xhr) {
            Swal.fire("Error", xhr.responseJSON?.message || "Gagal menyimpan.", "error");
        });
    });
});