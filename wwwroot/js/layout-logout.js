(function () {
    'use strict';

    document.addEventListener('click', function (e) {
        const btn = e.target.closest('#btnLogout');
        if (!btn) return;

        e.preventDefault();

        Swal.fire({
            title: 'Logout',
            text: 'Are you sure you want to logout?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Yes, logout',
            cancelButtonText: 'Cancel'
        }).then(function (result) {
            if (!result.isConfirmed) return;

            fetch('/Auth/Logout', {
                method: 'POST'
            })
            .then(() => {
                localStorage.removeItem("mdgs_token");
                window.location.href = '/Auth/Index';
            })
            .catch(() => {
                window.location.href = '/Auth/Index';
            });
        });
    });
})();
