// ─── Load Advanced yang di realisasikan (UangMukaRelatedId) Select2 ─────
function initUangMukaRelatedSelect(selectedId, selectedText) {
    const $sel = $('#UangMukaRelatedId');
    $sel.select2('destroy');
    $sel.empty();
    $sel.select2({
        theme: 'bootstrap-5',
        placeholder: '-- Pilih Advanced --',
        allowClear: true,
        ajax: {
            url: '/api/uang-muka/advanced-for-realisasi',
            dataType: 'json',
            delay: 250,
            data: function (params) {
                return { search: params.term };
            },
            processResults: function (data) {
                return {
                    results: data.map(function (item) {
                        return {
                            id: item.id,
                            text: item.atasNama + ' - ' + item.amount + ' - ' + item.budgetCode
                        };
                    })
                };
            },
            cache: true
        },
        width: '100%',
        dropdownParent: $('#uangMukaModal')
    });
    if (selectedId && selectedText) {
        var option = new Option(selectedText, selectedId, true, true);
        $sel.append(option).trigger('change');
    }
}

// Initialize Select2 for UangMukaRelatedId (initial)
$('#UangMukaRelatedId').select2({
    theme: 'bootstrap-5',
    placeholder: '-- Pilih Advanced --',
    allowClear: true,
    ajax: {
        url: '/api/uang-muka/advanced-for-realisasi',
        dataType: 'json',
        delay: 250,
        data: function (params) {
            return { search: params.term };
        },
        processResults: function (data) {
            return {
                results: data.map(function (item) {
                    return {
                        id: item.id,
                        text: item.atasNama + ' - ' + item.amount + ' - ' + item.budgetCode
                    };
                })
            };
        },
        cache: true
    },
    width: '100%'
});

// Show/hide field based on Jenis selection
$('#jenis').on('change', function () {
    if ($(this).val() === 'Realisasi') {
        $('#UangMukaRelatedIdWrapper').show();
    } else {
        $('#UangMukaRelatedIdWrapper').hide();
        $('#UangMukaRelatedId').val(null).trigger('change');
    }
});

// ─── KEY FIX: Hanya trigger jenis change saat modal CREATE (bukan Edit) ──────
$('#uangMukaModal').on('shown.bs.modal', function () {
    if (!$("#uangMukaId").val()) {
        $('#jenis').trigger('change');
    }
});

// If editing, set value for Select2
window.setUangMukaRelatedId = function (id, text) {
    if (id && text) {
        var option = new Option(text, id, true, true);
        $('#UangMukaRelatedId').append(option).trigger('change');
    }
}

$(function () {
    const apiBase = "/api/uang-muka";
    let uangMukaTable;

    // ─── Pindahkan modal langsung ke document.body ────────────────────────────
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
    function loadBudgetCodes(jenis) {
        return $.getJSON("/api/budget-codes?page=1&pageSize=2000").then(function (res) {
            const items = Array.isArray(res) ? res : (res.items || res.data || []);
            const $sel = $("#budgetCodeId");
            if ($sel.hasClass("select2-hidden-accessible")) $sel.select2("destroy");
            $sel.empty();
            // Tambahkan placeholder hanya jika single select
            if (jenis === 'Advanced') {
                $sel.append('<option value="">-- Pilih Budget Code --</option>');
            }
            items.forEach(function (b) {
                $sel.append(`<option value="${b.budgetCodeId}">${b.code} - ${b.description}</option>`);
            });
            // Multiple select jika bukan Advanced
            $sel.prop('multiple', jenis !== 'Advanced');
            $sel.select2({
                theme: "bootstrap-5",
                placeholder: "-- Pilih Budget Code --",
                allowClear: true,
                width: "100%",
                dropdownParent: $("#uangMukaModal"),
                closeOnSelect: jenis === 'Advanced',
                // Remove placeholder from selection
                templateSelection: function (data, container) {
                    if (data.id === "") return '';
                    return data.text;
                }
            });
            // Fix overlapping/tumpang tindih
            setTimeout(function() {
                $sel.next('.select2-container').css('min-width', '100%');
                $sel.next('.select2-container').find('.select2-selection--multiple').css({'min-height':'38px','padding':'4px 8px'});
            }, 100);
        });
    }

    // ─── Load COA Text (Vendor Categories) + Select2 ─────────────────────────
    function loadCoaTexts(jenis) {
        return $.getJSON("/api/vendor-categories?page=1&pageSize=2000").then(function (res) {
            const items = Array.isArray(res) ? res : (res.items || res.data || []);
            const $sel = $("#coaTextId");
            if ($sel.hasClass("select2-hidden-accessible")) $sel.select2("destroy");
            $sel.empty();
            if (jenis === 'Advanced') {
                $sel.append('<option value="">-- Pilih COA Text --</option>');
            }
            items.forEach(function (v) {
                $sel.append(`<option value="${v.vendorCategoryId}">${v.parentBudgetCodeLabel || "-"} - ${v.name}</option>`);
            });
            $sel.prop('multiple', jenis !== 'Advanced');
            $sel.select2({
                theme: "bootstrap-5",
                placeholder: "-- Pilih COA Text --",
                allowClear: true,
                width: "100%",
                dropdownParent: $("#uangMukaModal"),
                closeOnSelect: jenis === 'Advanced',
                templateSelection: function (data, container) {
                    if (data.id === "") return '';
                    return data.text;
                }
            });
            setTimeout(function() {
                $sel.next('.select2-container').css('min-width', '100%');
                $sel.next('.select2-container').find('.select2-selection--multiple').css({'min-height':'38px','padding':'4px 8px'});
            }, 100);
        });
    }

    // ─── Load Status Options — hanya isi <option>, TANPA init Select2 ─────────
    function loadStatusOptions(jenis) {
        const url = jenis === "Advanced"
            ? "/api/advanced-statuses?page=1&pageSize=2000"
            : "/api/biaya-realisasi-statuses?page=1&pageSize=2000";

        return $.getJSON(url).then(function (res) {
            const items = Array.isArray(res) ? res : (res.items || res.data || []);
            const $sel = $("#statusId");

            // Destroy Select2 dulu sebelum manipulasi options
            if ($sel.hasClass("select2-hidden-accessible")) {
                $sel.select2("destroy");
            }

            $sel.empty().append('<option value="">-- Pilih Status --</option>');
            items.forEach(function (s) {
                const id = jenis === "Advanced" ? s.advancedStatusId : s.biayaRealisasiStatusId;
                $sel.append(`<option value="${id}">${s.code} - ${s.name}</option>`);
            });
            // Tidak init Select2 di sini
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
        autoWidth: false,
        ajax: {
            url: apiBase,
            data: function (d) {
                // Filtering: only send userId if NOT admin (biar backend tahu)
                const userId = (typeof currentUserId === 'function' ? currentUserId() : $("#currentUserId").val()) || "";
                const isAdmin = (typeof isCurrentUserAdmin === 'function' ? isCurrentUserAdmin() : $("#currentUserIsAdmin").val() === "true");
                if (!isAdmin && userId) {
                    d.userId = userId;
                }
            },
            dataSrc: function (json) {
                console.log('DEBUG DataTable AJAX response:', json);
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
            {
                data: null,
                render: function (data, type, row) {
                    // Tampilkan semua deskripsi jika array, fallback ke properti tunggal jika ada
                    if (Array.isArray(row.budgetCodes) && row.budgetCodes.length > 0) {
                        return row.budgetCodes.map(b => b?.description || "").filter(Boolean).join(", ");
                    }
                    if (row.budgetCode && row.budgetCode.description) {
                        return row.budgetCode.description;
                    }
                    return "-";
                },
                defaultContent: "-"
            },
            {
                data: null,
                render: function (data, type, row) {
                    if (Array.isArray(row.coaTexts) && row.coaTexts.length > 0) {
                        return row.coaTexts.map(c => c?.name || "").filter(Boolean).join(", ");
                    }
                    if (row.coaText && row.coaText.name) {
                        return row.coaText.name;
                    }
                    return "-";
                },
                defaultContent: "-"
            },
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
                width: "400px",
                render: function (data, type, row) {
                    return `
                        <button class="btn btn-sm btn-light-info btn-detail" data-id="${row.uangMukaId}">
                            <i class="feather-eye me-1"></i>Detail
                        </button>
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
            $("#statusId")[0].value = "";
            $("#budgetCodeId").val("").trigger("change");
            $("#coaTextId").val("").trigger("change");
            $('#UangMukaRelatedIdWrapper').hide();
            $('#UangMukaRelatedId').val(null).trigger('change');
            showModal();
        });
    });

    // ─── Event: Jenis berubah → reload status & show/hide Advanced Realisasi + handle select2 single/multiple ──
    // Hanya untuk interaksi user di form, bukan saat edit load
    $("#jenis").on("change", function () {
        const val = $(this).val();
        if (!val) return;

        // Reload status options
        loadStatusOptions(val).then(function () {
            $("#statusId")[0].value = "";
        });

        // Handle UangMukaRelatedId tampil hanya untuk Realisasi
        if (val === "Realisasi") {
            $('#UangMukaRelatedIdWrapper').show();
            initUangMukaRelatedSelect();
        } else {
            $('#UangMukaRelatedIdWrapper').hide();
            $('#UangMukaRelatedId').val(null).trigger('change');
        }

        // --- FIX: Budget Code & COA Text single/multiple select2 ---
        // Destroy select2 dulu
        if ($('#budgetCodeId').hasClass('select2-hidden-accessible')) $('#budgetCodeId').select2('destroy');
        if ($('#coaTextId').hasClass('select2-hidden-accessible')) $('#coaTextId').select2('destroy');

        // Set attribute multiple sesuai jenis
        if (val === 'Biaya' || val === 'Realisasi') {
            $('#budgetCodeId').attr('multiple', 'multiple');
            $('#coaTextId').attr('multiple', 'multiple');
        } else {
            $('#budgetCodeId').removeAttr('multiple');
            $('#coaTextId').removeAttr('multiple');
        }

        // Clear value setiap ganti jenis
        $('#budgetCodeId').val(null);
        $('#coaTextId').val(null);

        // Re-init select2
        $('#budgetCodeId').select2({
            theme: 'bootstrap-5',
            width: '100%',
            dropdownParent: $('#uangMukaModal'),
            placeholder: '-- Pilih Budget Code --',
            allowClear: true,
            closeOnSelect: val === 'Advanced'
        });
        $('#coaTextId').select2({
            theme: 'bootstrap-5',
            width: '100%',
            dropdownParent: $('#uangMukaModal'),
            placeholder: '-- Pilih COA Text --',
            allowClear: true,
            closeOnSelect: val === 'Advanced'
        });
    });

    // ─── Event: Tombol Edit ───────────────────────────────────────────────────
    $("#unagMukasTable").on("click", ".btn-edit", function () {
        const id = $(this).data("id");
        $.get(`${apiBase}/${id}`).done(function (data) {
            portalModalToBody('uangMukaModal');

            // Isi semua field biasa
            $("#uangMukaId").val(data.uangMukaId);
            $("#atasNama").val(data.atasNama);
            $("#amount").val(data.amount);
            $("#startDate").val(data.startDate ? data.startDate.split("T")[0] : "");
            $("#endDate").val(data.endDate ? data.endDate.split("T")[0] : "");
            $("#noSAP").val(data.noSAP || "");
            $("#deskripsi").val(data.deskripsi || "");
            $("#uangMukaModalLabel").text("Edit Uang Muka");

            // ─── Set jenis TANPA trigger change ───────────────────────────────
            $("#jenis").val(data.jenis);

            // Show/hide wrapper manual — tidak lewat trigger change
            if (data.jenis === "Realisasi") {
                $('#UangMukaRelatedIdWrapper').show();
            } else {
                $('#UangMukaRelatedIdWrapper').hide();
            }

            Promise.all([
                loadBudgetCodes(data.jenis),
                loadCoaTexts(data.jenis),
                loadStatusOptions(data.jenis)
            ]).then(function () {
                // Set value: jika array, set multiple; jika single, set satu
                if (data.jenis === "Advanced") {
                    // Single select
                    let bc = Array.isArray(data.budgetCodeIds) ? data.budgetCodeIds[0] : (data.budgetCodeId || "");
                    let ct = Array.isArray(data.coaTextIds) ? data.coaTextIds[0] : (data.coaTextId || "");
                    $("#budgetCodeId").val(bc ? [bc] : [""]).trigger("change");
                    $("#coaTextId").val(ct ? [ct] : [""]).trigger("change");
                } else {
                    if (Array.isArray(data.budgetCodeIds)) {
                        $("#budgetCodeId").val(data.budgetCodeIds).trigger("change");
                    } else if (data.budgetCodeId) {
                        $("#budgetCodeId").val([data.budgetCodeId]).trigger("change");
                    } else {
                        $("#budgetCodeId").val("").trigger("change");
                    }
                    if (Array.isArray(data.coaTextIds)) {
                        $("#coaTextId").val(data.coaTextIds).trigger("change");
                    } else if (data.coaTextId) {
                        $("#coaTextId").val([data.coaTextId]).trigger("change");
                    } else {
                        $("#coaTextId").val("").trigger("change");
                    }
                }

                // ─── KEY FIX: Set statusId pakai native select — tanpa Select2 ──
                // Ini menghindari semua timing & re-init issue Select2
                const $status = $("#statusId");
                if ($status.hasClass("select2-hidden-accessible")) {
                    $status.select2("destroy");
                }
                $status[0].value = data.statusId;
                console.log('DEBUG statusId:', data.statusId, '→ result:', $status[0].value);

                // Advanced Realisasi
                if (data.jenis === "Realisasi") {
                    $('#UangMukaRelatedIdWrapper').show();
                    let advText = data.uangMukaRelatedText || '';
                    if (!advText && data.uangMukaRelatedId) {
                        $.get(`/api/uang-muka/${data.uangMukaRelatedId}`).done(function (adv) {
                            advText = adv.atasNama + ' - ' + adv.amount + ' - ' + (adv.budgetCode?.description || '');
                            initUangMukaRelatedSelect(data.uangMukaRelatedId, advText);
                        });
                    } else {
                        initUangMukaRelatedSelect(data.uangMukaRelatedId, advText);
                    }
                } else {
                    $('#UangMukaRelatedIdWrapper').hide();
                    $('#UangMukaRelatedId').val(null).trigger('change');
                }

                // showModal SATU KALI di akhir
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
        const jenis = $("#jenis").val();
        // Ambil value BudgetCode dan COA Text
        let budgetCodeVal = $("#budgetCodeId").val();
        let coaTextVal = $("#coaTextId").val();
        // Untuk Advanced: single, untuk lain: array
        let payload = {
            uangMukaId: id || undefined,
            jenis: jenis,
            atasNama: $("#atasNama").val(),
            amount: parseFloat($("#amount").val()),
            startDate: $("#startDate").val(),
            endDate: $("#endDate").val(),
            statusId: $("#statusId").val(),
            noSAP: $("#noSAP").val(),
            deskripsi: $("#deskripsi").val(),
            uangMukaRelatedId: (jenis === "Realisasi" ? $("#UangMukaRelatedId").val() : null)
        };
        if (jenis === "Advanced") {
            // Single select, kirim array berisi satu (atau null)
            payload.budgetCodeIds = budgetCodeVal && budgetCodeVal !== "" ? [budgetCodeVal] : [];
            payload.coaTextIds = coaTextVal && coaTextVal !== "" ? [coaTextVal] : [];
        } else {
            // Multiple select
            payload.budgetCodeIds = Array.isArray(budgetCodeVal) ? budgetCodeVal : (budgetCodeVal ? [budgetCodeVal] : []);
            payload.coaTextIds = Array.isArray(coaTextVal) ? coaTextVal : (coaTextVal ? [coaTextVal] : []);
        }

        const method = id ? "PUT" : "POST";
        const url = id ? `${apiBase}/${id}` : apiBase;

        $.ajax({
            url: url,
            method: method,
            contentType: "application/json",
            data: JSON.stringify(payload)
        }).done(async function (res) {
            // If Add (POST), get new ID from response and upload attachments before closing modal
            let newId = id;
            if (!id && res && (res.uangMukaId || res.id)) {
                newId = res.uangMukaId || res.id;
            }
            if (newId && typeof window.uploadPendingFiles === 'function') {
                await window.uploadPendingFiles(newId);
            }
            hideModal();
            reloadTable();
            Swal.fire("Saved", "Uang Muka berhasil disimpan.", "success");
        }).fail(function (xhr) {
            Swal.fire("Error", xhr.responseJSON?.message || "Gagal menyimpan.", "error");
        });
    });
});