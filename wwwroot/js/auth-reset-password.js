// Reset Password Form Handler

$(document).ready(function () {
    console.log('Reset Password Form Initialized');

    // Toggle password visibility
    $('#toggleNewPassword').on('click', function () {
        togglePasswordVisibility('#inputNewPassword', '#toggleNewPassword');
    });

    $('#toggleConfirmPassword').on('click', function () {
        togglePasswordVisibility('#inputConfirmPassword', '#toggleConfirmPassword');
    });

    // Submit form
    $('#resetPasswordForm').on('submit', function (e) {
        e.preventDefault();
        handleResetPassword();
    });
});

function handleResetPassword() {
    const token = $('#resetToken').val();
    const newPassword = $('#inputNewPassword').val();
    const confirmPassword = $('#inputConfirmPassword').val();

    // Validation
    if (!newPassword) {
        showAlert('danger', 'New password is required');
        $('#inputNewPassword').focus();
        return;
    }

    if (newPassword.length < 6) {
        showAlert('danger', 'Password must be at least 6 characters long');
        $('#inputNewPassword').focus();
        return;
    }

    if (!confirmPassword) {
        showAlert('danger', 'Please confirm your password');
        $('#inputConfirmPassword').focus();
        return;
    }

    if (newPassword !== confirmPassword) {
        showAlert('danger', 'Passwords do not match');
        $('#inputConfirmPassword').focus();
        return;
    }

    // Disable button and show loading
    const btnSubmit = $('#btnSubmit');
    const originalText = btnSubmit.html();
    btnSubmit.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span>Resetting...');

    // Clear previous alerts
    $('#resetPasswordAlertPlaceholder').empty();

    // Call API
    $.ajax({
        url: '/api/auth/reset-password',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            token: token,
            newPassword: newPassword
        }),
        success: function (response) {
            console.log('Reset password response:', response);
            
            if (response.success) {
                // Show success message
                Swal.fire({
                    icon: 'success',
                    title: 'Password Reset Successful!',
                    html: 'Your password has been reset successfully.<br><br>You can now login with your new password.',
                    confirmButtonColor: '#0d6efd',
                    confirmButtonText: 'Go to Login',
                    allowOutsideClick: false,
                    allowEscapeKey: false
                }).then((result) => {
                    // Redirect to login page
                    window.location.href = '/Auth/Index';
                });
            } else {
                showAlert('danger', response.message || 'Failed to reset password');
                btnSubmit.prop('disabled', false).html(originalText);
            }
        },
        error: function (xhr, status, error) {
            console.error('Reset password error:', error);
            let errorMessage = 'Failed to reset password. Please try again.';
            
            if (xhr.responseJSON) {
                if (xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                } else if (xhr.responseJSON.errors) {
                    errorMessage = Object.values(xhr.responseJSON.errors).flat().join('<br>');
                }
            } else if (xhr.status === 400) {
                errorMessage = 'Invalid or expired reset token. Please request a new password reset.';
            } else if (xhr.status === 500) {
                errorMessage = 'Server error. Please try again later.';
            }
            
            showAlert('danger', errorMessage);
            btnSubmit.prop('disabled', false).html(originalText);
        }
    });
}

function togglePasswordVisibility(inputSelector, buttonSelector) {
    const input = $(inputSelector);
    const button = $(buttonSelector);
    const icon = button.find('i');

    if (input.attr('type') === 'password') {
        input.attr('type', 'text');
        icon.removeClass('feather-eye').addClass('feather-eye-off');
    } else {
        input.attr('type', 'password');
        icon.removeClass('feather-eye-off').addClass('feather-eye');
    }
}

function showAlert(type, message) {
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            <i class="feather-${type === 'success' ? 'check-circle' : 'alert-circle'} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    $('#resetPasswordAlertPlaceholder').html(alertHtml);
}
