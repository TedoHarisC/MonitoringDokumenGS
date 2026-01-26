// Forgot Password Form Handler

$(document).ready(function () {
    console.log('Forgot Password Form Initialized');

    $('#forgotPasswordForm').on('submit', function (e) {
        e.preventDefault();
        handleForgotPassword();
    });
});

function handleForgotPassword() {
    const username = $('#inputUsername').val().trim();
    const email = $('#inputEmail').val().trim();

    // Validation
    if (!username) {
        showAlert('danger', 'Username is required');
        $('#inputUsername').focus();
        return;
    }

    if (!email) {
        showAlert('danger', 'Email is required');
        $('#inputEmail').focus();
        return;
    }

    if (!isValidEmail(email)) {
        showAlert('danger', 'Please enter a valid email address');
        $('#inputEmail').focus();
        return;
    }

    // Disable button and show loading
    const btnSubmit = $('#btnSubmit');
    const originalText = btnSubmit.html();
    btnSubmit.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span>Sending...');

    // Clear previous alerts
    $('#forgotPasswordAlertPlaceholder').empty();

    // Call API
    $.ajax({
        url: '/api/auth/forgot-password',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            username: username,
            email: email
        }),
        success: function (response) {
            console.log('Forgot password response:', response);
            
            if (response.success) {
                // Show success message
                Swal.fire({
                    icon: 'success',
                    title: 'Email Sent!',
                    html: 'We\'ve sent password reset instructions to your email.<br><br><strong>Please check your inbox and spam folder.</strong>',
                    confirmButtonColor: '#0d6efd',
                    confirmButtonText: 'OK'
                }).then((result) => {
                    // Redirect to login page after success
                    window.location.href = '/Auth/Index';
                });
            } else {
                showAlert('danger', response.message || 'Failed to send reset email');
                btnSubmit.prop('disabled', false).html(originalText);
            }
        },
        error: function (xhr, status, error) {
            console.error('Forgot password error:', error);
            let errorMessage = 'Failed to send reset email. Please try again.';
            
            if (xhr.responseJSON) {
                if (xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                } else if (xhr.responseJSON.errors) {
                    errorMessage = Object.values(xhr.responseJSON.errors).flat().join('<br>');
                }
            } else if (xhr.status === 404) {
                errorMessage = 'User not found. Please check your username and email.';
            } else if (xhr.status === 500) {
                errorMessage = 'Server error. Please try again later.';
            }
            
            showAlert('danger', errorMessage);
            btnSubmit.prop('disabled', false).html(originalText);
        }
    });
}

function showAlert(type, message) {
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            <i class="feather-${type === 'success' ? 'check-circle' : 'alert-circle'} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    $('#forgotPasswordAlertPlaceholder').html(alertHtml);
}

function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}
