// transaction-uang-muka-detail.js
// Handles detail modal for Uang Muka
$(function () {
    // Show detail modal
    $(document).on('click', '.btn-detail', function () {
        const id = $(this).data('id');
        if (!id) return;
        // Portal modal to body to avoid overlay issues
        const modalEl = document.getElementById('uangMukaDetailModal');
        if (modalEl && modalEl.parentElement !== document.body) {
            document.body.appendChild(modalEl);
        }
        loadUangMukaDetail(id);
    });

    async function loadUangMukaDetail(id) {
        const $modal = $('#uangMukaDetailModal');
        const $content = $('#uangMukaDetailContent');
        $content.html('<div class="text-center py-5"><div class="spinner-border"></div></div>');
        $modal.modal('show');
        try {
            // Get main data
            const res = await authFetch(`/api/uang-muka/${id}`);
            if (!res.ok) throw new Error('Gagal load data');
            const data = await res.json();
            let relatedHtml = '';
            if (data.jenis === 'Realisasi' && data.uangMukaRelatedId) {
                // Load related Advanced
                const relRes = await authFetch(`/api/uang-muka/${data.uangMukaRelatedId}`);
                if (relRes.ok) {
                    const rel = await relRes.json();
                    // Fetch related attachments
                    let relAttHtml = '';
                    try {
                        const relAttRes = await authFetch(`/api/attachments/by-reference/${rel.uangMukaId}`);
                        if (relAttRes.ok) {
                            const relAtts = await relAttRes.json();
                            if (Array.isArray(relAtts) && relAtts.length > 0) {
                                relAttHtml = `<div class=\"mt-2\"><b>Attachments Advanced:</b><ul>` +
                                    relAtts.map(a => `<li><a href=\"/api/attachments/download/${a.attachmentId}\" target=\"_blank\" download>${a.fileName}</a></li>`).join('') +
                                    `</ul></div>`;
                            } else {
                                relAttHtml = '<div class=\"mt-2\"><b>Attachments Advanced:</b> <span class=\"text-muted\">No attachments</span></div>';
                            }
                        }
                    } catch {}
                    relatedHtml = `<div class=\"mb-3\"><b>Advanced Terkait:</b><br>
                        Atas Nama: ${rel.atasNama}<br>
                        Amount: ${rel.amount}<br>
                        Budget Code: ${rel.budgetCode?.description || '-'}<br>
                        COA Text: ${rel.coaText?.name || '-'}<br>
                        Start Date: ${rel.startDate ? rel.startDate.split('T')[0] : '-'}<br>
                        End Date: ${rel.endDate ? rel.endDate.split('T')[0] : '-'}
                        ${relAttHtml}
                    </div>`;
                }
            }
            // Get attachments
            let attachmentsHtml = '';
            try {
                const attRes = await authFetch(`/api/attachments/by-reference/${id}`);
                if (attRes.ok) {
                    const atts = await attRes.json();
                    if (Array.isArray(atts) && atts.length > 0) {
                        attachmentsHtml = `<div class="mb-3"><b>Attachments:</b><ul>` +
                            atts.map(a => `<li><a href="/api/attachments/download/${a.attachmentId}" target="_blank" download>${a.fileName}</a></li>`).join('') +
                            `</ul></div>`;
                    } else {
                        attachmentsHtml = '<div class="mb-3"><b>Attachments:</b> <span class="text-muted">No attachments</span></div>';
                    }
                }
            } catch {}
            // Helper: badge color for status
                                            function getStatusBadgeClass(status) {
                                                if (!status) return 'bg-secondary';
                                                const s = status.toLowerCase();
                                                if (s.includes('done') || s.includes('selesai') || s.includes('lunas')) return 'bg-success';
                                                if (s.includes('butuh') || s.includes('realisasi') || s.includes('belum')) return 'bg-warning text-dark';
                                                return 'bg-info text-dark';
                                            }
            $content.html(`
                <div class="container-fluid p-0">
                    <div class="row g-4">
                        <div class="col-12 col-lg-7">
                            <div class="card shadow-sm border-0 mb-4">
                                <div class="card-body">
                                    <div class="d-flex align-items-center mb-3 gap-3">
                                        <div class="flex-shrink-0">
                                            <div class="rounded-circle bg-primary-soft d-flex align-items-center justify-content-center"
                                                style="width:54px;height:54px;">
                                                <i class="feather-user text-primary fs-3"></i>
                                            </div>
                                        </div>
                                        <div>
                                            <div class="fw-bold fs-4 mb-1">${data.atasNama}</div>
                                            <span class="badge bg-secondary me-2">${data.jenis}</span>
                                            <span class="badge ${getStatusBadgeClass(data.status)}">${data.status || '-'}</span>
                                        </div>
                                    </div>
                                    <div class="row g-2 mb-2">
                                        <div class="col-6">
                                            <div class="text-muted small">Amount</div>
                                            <div class="fw-bold text-success fs-5">${data.amount?.toLocaleString('id-ID', {style:'currency',currency:'IDR'}) || '-'}</div>
                                        </div>
                                        <div class="col-6">
                                            <div class="text-muted small">No SAP</div>
                                            <div class="fw-bold">${data.noSAP || '-'}</div>
                                        </div>
                                    </div>
                                    <div class="row g-2 mb-2">
                                        <div class="col-6">
                                            <div class="text-muted small">Budget Code</div>
                                            <div class="fw-bold">
                                                ${Array.isArray(data.budgetCodes) && data.budgetCodes.length > 0
                                                    ? data.budgetCodes.map(b => b?.description || "").filter(Boolean).join(", ")
                                                    : (data.budgetCode?.description || '-')}
                                            </div>
                                        </div>
                                        <div class="col-6">
                                            <div class="text-muted small">COA Text</div>
                                            <div class="fw-bold">
                                                ${Array.isArray(data.coaTexts) && data.coaTexts.length > 0
                                                    ? data.coaTexts.map(c => c?.name || "").filter(Boolean).join(", ")
                                                    : (data.coaText?.name || '-')}
                                            </div>
                                        </div>
                                    </div>
                                    <div class="row g-2 mb-2">
                                        <div class="col-6">
                                            <div class="text-muted small">Start Date</div>
                                            <div class="fw-bold">${data.startDate ? data.startDate.split('T')[0] : '-'}</div>
                                        </div>
                                        <div class="col-6">
                                            <div class="text-muted small">End Date</div>
                                            <div class="fw-bold">${data.endDate ? data.endDate.split('T')[0] : '-'}</div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="col-12 col-lg-5">
                            <div class="card shadow-sm border-0 mb-4">
                                <div class="card-body">
                                    <h6 class="text-primary mb-2"><i class="feather-file-text me-1"></i> Deskripsi</h6>
                                    <div class="bg-light rounded p-2 border mb-2">${data.deskripsi || '<span class=\"text-muted\">-</span>'}</div>
                                    ${relatedHtml}
                                </div>
                            </div>
                            <div class="card shadow-sm border-0">
                                <div class="card-body">
                                    <h6 class="text-primary mb-2"><i class="feather-paperclip me-1"></i> Attachments</h6>
                                    ${attachmentsHtml}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            `);

            // Timeline History Section
            $content.append(`
                <div class="mt-5">
                    <h6 class="text-primary mb-3"><i class="feather-clock me-1"></i> History Perubahan Status</h6>
                    <div id="uangMukaHistoryTimeline" class="timeline-container"></div>
                </div>
            `);
            // Load timeline after detail
            if (id) window.renderUangMukaDetailHistory && window.renderUangMukaDetailHistory(id);

            console.log(data);
        } catch (e) {
            $content.html('<div class="alert alert-danger">Gagal memuat detail.</div>');
        }
    }
});

// Timeline History Loader for Uang Muka Detail
function loadUangMukaHistory(uangMukaId) {
    const $timeline = $("#uangMukaHistoryTimeline");
    $timeline.html('<div class="text-muted">Loading history...</div>');
    $.get(`/api/uang-muka/${uangMukaId}/history`)
        .done(function (data) {
            if (!data || !Array.isArray(data) || data.length === 0) {
                $timeline.html('<div class="text-muted">Tidak ada history perubahan.</div>');
                return;
            }
            // Sort by CreatedAt ascending
            data.sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt));
            let html = '<div class="timeline">';
            data.forEach(function (item, idx) {
                // Ambil status baru dan user
                let status = "";
                let user = item.userName || (item.userId ? `User: ${item.userId}` : "-");
                let tgl = new Date(item.createdAt).toLocaleString();
                // Cek perubahan status
                try {
                    if (item.newData) {
                        const newData = typeof item.newData === 'string' ? JSON.parse(item.newData) : item.newData;
                        status = newData.status || newData.Status || newData.statusId || newData.StatusId || "-";
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

// Hook: panggil ini setelah detail loaded
window.renderUangMukaDetailHistory = function(uangMukaId) {
    loadUangMukaHistory(uangMukaId);
};
