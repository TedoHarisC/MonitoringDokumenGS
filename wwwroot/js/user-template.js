$(document).ready(function () {
    // Ambil data template dengan permission 'user'
    $.get('/api/template-files/permission/user', function (data) {
        var html = '';
        if (data && data.length > 0) {
            data.forEach(function (item) {
                var ext = item.fileName ? item.fileName.split('.').pop().toLowerCase() : '';
                var icon = getFileIcon(ext);
                var size = formatFileSize(item.fileSize);
                var uploaded = item.createdAt ? formatDateIndo(item.createdAt) : '-';
                var fileName = item.fileName ? item.fileName : '-';
                html += `
                <div class="col-12 col-sm-6 col-md-4 col-lg-3 mb-4">
                    <div class="card h-100 shadow-sm border-0 template-card hover-shadow" style="cursor:pointer" onclick="window.open('/api/template-files/download/${item.id}', '_blank')">
                        <div class="card-body d-flex flex-column align-items-center justify-content-center py-3">
                            <div class="file-icon mb-3">${icon}</div>
                            <div class="fs-5 fw-bold text-center mb-3">${item.title}</div>
                            <div class="w-100 px-2">
                                <div class="fw-semibold small mb-1" style="text-align:left">Ukuran: ${size}</div>
                                <div class="text-muted small mb-1" style="text-align:left">Nama file: ${fileName}</div>
                                <div class="text-muted small" style="text-align:left">Diupload: ${uploaded}</div>
                            </div>
                        </div>
                    </div>
                </div>
                `;
                function formatDateIndo(dateStr) {
                    if (!dateStr) return '-';
                    var d = new Date(dateStr);
                    if (isNaN(d)) return '-';
                    var options = { year: 'numeric', month: 'long', day: 'numeric' };
                    return d.toLocaleDateString('id-ID', options);
                }
            });
        } else {
            html = '<div class="col-12"><div class="d-flex flex-column align-items-center justify-content-center min-vh-60" style="height:60vh"><i class="bi bi-folder-x text-secondary mb-3" style="font-size:3rem"></i><div class="text-muted fs-5">Belum ada dokumen yang diupload oleh GS.</div></div></div>';
        }
        $('#userTemplateList').html(html);
    });

    function getFileIcon(ext) {
        switch (ext) {
            case 'pdf':
                return '<i class="bi bi-file-earmark-pdf text-danger" style="font-size:4rem"></i>';
            case 'xls':
            case 'xlsx':
                return '<i class="bi bi-file-earmark-excel text-success" style="font-size:4rem"></i>';
            case 'doc':
            case 'docx':
                return '<i class="bi bi-file-earmark-word text-primary" style="font-size:4rem"></i>';
            case 'jpg':
            case 'jpeg':
            case 'png':
                return '<i class="bi bi-file-earmark-image text-warning" style="font-size:4rem"></i>';
            default:
                return '<i class="bi bi-file-earmark text-secondary" style="font-size:4rem"></i>';
        }
    }
    function formatFileSize(size) {
        if (!size || isNaN(size)) return '-';
        if (size < 1024) return size + ' B';
        if (size < 1024 * 1024) return (size / 1024).toFixed(1) + ' KB';
        return (size / 1024 / 1024).toFixed(2) + ' MB';
    }
});
