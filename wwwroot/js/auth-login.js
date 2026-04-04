/*
  auth-login.js
  - Handles login form submission
  - Uses jQuery for DOM/ajax and SweetAlert2 for user-friendly alerts
  - Stores tokens in localStorage (consider using HttpOnly cookies in production)
*/

(function ($) {
  "use strict";

  function showInlineAlert(html, type = "danger") {
    const alert =
      `<div class="alert alert-${type} alert-dismissible" role="alert">` +
      html +
      '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
      "</div>";
    $("#loginAlertPlaceholder").html(alert);
  }

  function clearInlineAlert() {
    $("#loginAlertPlaceholder").empty();
  }

  $(function () {
    const $form = $("#loginForm");

    // Toggle password visibility
    $("#togglePassword").on("click", function () {
      const $input = $("#inputPassword");
      const $icon = $("#togglePasswordIcon");
      if ($input.attr("type") === "password") {
        $input.attr("type", "text");
        $icon.removeClass("feather-eye").addClass("feather-eye-off");
      } else {
        $input.attr("type", "password");
        $icon.removeClass("feather-eye-off").addClass("feather-eye");
      }
    });

    $form.on("submit", function (e) {
      e.preventDefault();
      clearInlineAlert();

      const username = $("#inputUsername").val()?.toString().trim() ?? "";
      const password = $("#inputPassword").val()?.toString() ?? "";

      if (!username || !password) {
        showInlineAlert("Username and password are required.", "warning");
        return;
      }

      // disable button
      const $btn = $("#btnLogin")
        .attr("disabled", "disabled")
        .text("Signing in...");

      $.ajax({
        url: "/Auth/Login",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify({ username: username, password: password }),
        dataType: "json",
      })
        .done(function (resp) {
          if (!resp || resp.success !== true) {
            Swal.fire({
              icon: "error",
              title: "Login failed",
              text: resp?.message || "Invalid credentials",
            });
            return;
          }

          Swal.fire({
            icon: "success",
            title: "Success",
            text: "Login successful",
            timer: 900,
            showConfirmButton: false,
          }).then(() => {
            localStorage.setItem("mdgs_token", resp.token);
            window.location.href = "/Home/Index";
          });
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
          let message = "An error occurred while signing in";
          try {
            const body =
              jqXHR.responseJSON ||
              (jqXHR.responseText && JSON.parse(jqXHR.responseText));
            if (body) {
              message =
                body.message ||
                (body.errors && body.errors.join(", ")) ||
                JSON.stringify(body);
            }
          } catch (e) {
            // ignore parse errors
          }

          // show inline and modal
          showInlineAlert(message, "danger");
          Swal.fire({ icon: "error", title: "Login error", text: message });
        })
        .always(function () {
          $btn.removeAttr("disabled").text("Login");
        });
    });
  });
})(jQuery);
