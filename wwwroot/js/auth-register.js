/*
  auth-register.js
  - Handles registration form submission
  - Uses jQuery for DOM/ajax and SweetAlert2 for UI
*/

;(function ($) {
    'use strict';

    function showInlineAlert(html, type = 'danger') {
        const alert = `<div class="alert alert-${type} alert-dismissible" role="alert">` +
            html +
            '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
            '</div>';
        $('#registerAlertPlaceholder').html(alert);
    }

    function clearInlineAlert() { $('#registerAlertPlaceholder').empty(); }

    $(function () {
        const $form = $('#registerForm');

        // Password strength UI elements
        const $password = $('#inputPassword');
        const $passwordConfirm = $('#inputPasswordConfirm');
        const $progressBar = $('.progress-bar');
        const $strengthBlocks = $progressBar.find('div');

        function resetStrength() {
            $strengthBlocks.css({ 'background-color': 'transparent' });
            $('#passwordStrengthText').text('').css('color', '');
        }

        function setStrength(score) {
            // score 0..4
            const colors = ['#e74c3c', '#e67e22', '#f1c40f', '#2ecc71'];
            const labels = ['Too short', 'Weak', 'Moderate', 'Strong', 'Very strong'];
            resetStrength();

            if (!score || score <= 0) {
                $('#passwordStrengthText').text(labels[0]).css('color', '#6c757d');
                return;
            }

            const color = colors[Math.max(0, score - 1)];
            for (let i = 0; i < Math.min(score, $strengthBlocks.length); i++) {
                $($strengthBlocks[i]).css('background-color', color);
            }

            $('#passwordStrengthText').text(`Strength: ${labels[Math.min(score, 4)]}`).css('color', color);
        }

        function evaluatePassword(pwd) {
            let score = 0;
            if (!pwd) return 0;
            if (pwd.length >= 8) score++;
            if (/[A-Z]/.test(pwd)) score++;
            if (/[0-9]/.test(pwd)) score++;
            if (/[^A-Za-z0-9]/.test(pwd)) score++;
            return score; // 0..4
        }

        // Utility: generate a random password using crypto where available
        function generatePassword(length = 12) {
            const lower = 'abcdefghijklmnopqrstuvwxyz';
            const upper = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
            const digits = '0123456789';
            const symbols = '!@#$%^&*()-_=+[]{};:,.<>?';
            const all = lower + upper + digits + symbols;

            const getRandom = (max) => {
                if (window.crypto && window.crypto.getRandomValues) {
                    const array = new Uint32Array(1);
                    window.crypto.getRandomValues(array);
                    return array[0] % max;
                }
                return Math.floor(Math.random() * max);
            };

            // ensure complexity: include at least one from each set
            let pwd = '';
            pwd += lower[getRandom(lower.length)];
            pwd += upper[getRandom(upper.length)];
            pwd += digits[getRandom(digits.length)];
            pwd += symbols[getRandom(symbols.length)];
            for (let i = 4; i < length; i++) pwd += all[getRandom(all.length)];

            // shuffle
            pwd = pwd.split('').sort(() => 0.5 - Math.random()).join('');
            return pwd;
        }

        // initialize strength UI and icons
        resetStrength();
        $('.show-pass').find('i').removeClass().addClass('feather-eye-off');

        // Generate password
        $(document).on('click', '.gen-pass', function () {
            const pwd = generatePassword(12);
            $password.val(pwd).trigger('input');
            $passwordConfirm.val(pwd);
        });

        // Show / hide password
        $(document).on('click', '.show-pass', function () {
            const $icon = $(this).find('i');
            const type = $password.attr('type') === 'password' ? 'text' : 'password';
            $password.attr('type', type);
            $passwordConfirm.attr('type', type);
            // toggle icon class (using feather icons in template)
            if (type === 'text') {
                $icon.removeClass().addClass('feather-eye');
            } else {
                $icon.removeClass().addClass('feather-eye-off');
            }
        });

        // Update strength on input
        $password.on('input', function () {
            const val = $(this).val()?.toString() ?? '';
            const score = evaluatePassword(val);
            setStrength(score);
        });


        $form.on('submit', function (e) {
            e.preventDefault();
            clearInlineAlert();

            const email = $('#inputEmail').val()?.toString().trim() ?? '';
            const username = $('#inputUsername').val()?.toString().trim() ?? '';
            const password = $('#inputPassword').val()?.toString() ?? '';
            const passwordConfirm = $('#inputPasswordConfirm').val()?.toString() ?? '';

            if (!email || !username || !password || !passwordConfirm) {
                showInlineAlert('Please fill all required fields.', 'warning');
                return;
            }

            if (password !== passwordConfirm) {
                showInlineAlert('Password and confirmation do not match.', 'warning');
                return;
            }

            const $btn = $('#btnRegister').attr('disabled', 'disabled').text('Creating...');

            const payload = {
                username: username,
                password: password,
                email: email,
                // vendorId optional, omitted unless needed
            };

            $.ajax({
                url: '/api/v1/Auth/register',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(payload),
                dataType: 'json'
            })
                .done(function (resp) {
                    const success = resp && (resp.success === true || resp.Success === true);
                    const message = (resp && (resp.message || resp.Message)) || 'Registration completed';

                    if (!success) {
                        showInlineAlert(message || 'Registration failed', 'danger');
                        Swal.fire({ icon: 'error', title: 'Register failed', text: message });
                        return;
                    }

                    Swal.fire({ icon: 'success', title: 'Registered', text: message }).then(() => {
                        // redirect to login
                        window.location.href = '/Auth/Index';
                    });
                })
                .fail(function (jqXHR) {
                    let message = 'An unexpected error occurred';
                    try {
                        const body = jqXHR.responseJSON || (jqXHR.responseText && JSON.parse(jqXHR.responseText));
                        if (body) message = body.message || body.Message || (body.errors && body.errors.join(', ')) || JSON.stringify(body);
                    } catch (e) { /* ignore */ }

                    showInlineAlert(message, 'danger');
                    Swal.fire({ icon: 'error', title: 'Error', text: message });
                })
                .always(function () { $btn.removeAttr('disabled').text('Create Account'); });
        });
    });

})(jQuery);
