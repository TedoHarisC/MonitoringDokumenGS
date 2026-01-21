/*
  auth-login.js
  - Handles login form submission
  - Uses jQuery for DOM/ajax and SweetAlert2 for user-friendly alerts
  - Stores tokens in localStorage (consider using HttpOnly cookies in production)
*/

;(function ($) {
    'use strict';

    function showInlineAlert(html, type = 'danger') {
        const alert = `<div class="alert alert-${type} alert-dismissible" role="alert">` +
            html +
            '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
            '</div>';
        $('#loginAlertPlaceholder').html(alert);
    }

    function clearInlineAlert() {
        $('#loginAlertPlaceholder').empty();
    }

    $(function () {
        const $form = $('#loginForm');

        $form.on('submit', function (e) {
            e.preventDefault();
            clearInlineAlert();

            const username = $('#inputUsername').val()?.toString().trim() ?? '';
            const password = $('#inputPassword').val()?.toString() ?? '';

            if (!username || !password) {
                showInlineAlert('Username and password are required.', 'warning');
                return;
            }

            // disable button
            const $btn = $('#btnLogin').attr('disabled', 'disabled').text('Signing in...');

            $.ajax({
                url: '/api/v1/Auth/login',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ username: username, password: password }),
                dataType: 'json'
            })
                .done(function (resp) {
                    // resp structure: { success, message, data } or direct { token, refreshToken }
                    let token = '';
                    let refresh = '';

                    if (resp) {
                        if (resp.data && resp.data.token) {
                            token = resp.data.token;
                            refresh = resp.data.refreshToken || resp.data.refresh || '';
                        } else if (resp.token) {
                            token = resp.token;
                            refresh = resp.refreshToken || resp.refresh || '';
                        }
                    }

                    if (!token) {
                        // try showing message from API
                        const message = (resp && (resp.message || (resp.errors && resp.errors.join(', ')))) || 'Invalid credentials';
                        Swal.fire({ icon: 'error', title: 'Login failed', text: message });
                        return;
                    }

                    // store tokens
                    try {
                        localStorage.setItem('access_token', token);
                        if (refresh) localStorage.setItem('refresh_token', refresh);
                    } catch (ex) {
                        console.warn('Unable to store tokens in localStorage', ex);
                    }

                    Swal.fire({ icon: 'success', title: 'Success', text: 'Login successful', timer: 900, showConfirmButton: false }).then(() => {
                        // Redirect to dashboard or home
                        window.location.href = '/';
                    });
                })
                .fail(function (jqXHR, textStatus, errorThrown) {
                    let message = 'An error occurred while signing in';
                    try {
                        const body = jqXHR.responseJSON || (jqXHR.responseText && JSON.parse(jqXHR.responseText));
                        if (body) {
                            message = body.message || (body.errors && body.errors.join(', ')) || JSON.stringify(body);
                        }
                    } catch (e) {
                        // ignore parse errors
                    }

                    // show inline and modal
                    showInlineAlert(message, 'danger');
                    Swal.fire({ icon: 'error', title: 'Login error', text: message });
                })
                .always(function () {
                    $btn.removeAttr('disabled').text('Login');
                });
        });
    });

})(jQuery);
