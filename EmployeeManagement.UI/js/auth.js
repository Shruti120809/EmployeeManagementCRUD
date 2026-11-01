(function () {
  const API_BASE_URL = "https://localhost:7039/api/Auth";
  const TOKEN_KEY = "employeeManagement.token";
  const USER_KEY = "employeeManagement.user";

  function redirectToLogin() {
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(USER_KEY);
    if (!/\/(login|register)\.html$/i.test(window.location.pathname)) {
      window.location.replace("login.html");
    }
  }

  window.EmployeeAuth = {
    token: function () {
      return sessionStorage.getItem(TOKEN_KEY);
    },
    requireAuth: function () {
      const token = sessionStorage.getItem(TOKEN_KEY);
      if (!token) {
        window.location.replace("login.html");
        return false;
      }

      try {
        const user = JSON.parse(sessionStorage.getItem(USER_KEY) || "{}");
        const name = typeof user.fullName === "string" ? user.fullName : "Employee Management user";
        $("#userLabel").text(name);
        $("#userAvatar").text(name.trim().split(/\s+/).slice(0, 2).map(function (part) {
          return part.charAt(0).toUpperCase();
        }).join("") || "U");
      } catch {
        redirectToLogin();
        return false;
      }
      return true;
    }
  };

  $(function () {
    $.ajaxSetup({
      beforeSend: function (xhr) {
        const token = sessionStorage.getItem(TOKEN_KEY);
        if (token) xhr.setRequestHeader("Authorization", "Bearer " + token);
      }
    });

    $(document).ajaxError(function (_event, xhr) {
      if (xhr.status === 401 && !/\/(login|register)\.html$/i.test(window.location.pathname)) {
        redirectToLogin();
      }
    });

    $(document).on("click", "#logoutButton", function () {
      redirectToLogin();
    });

    $("[data-toggle-password]").on("click", function () {
      const input = document.getElementById($(this).attr("data-toggle-password"));
      if (!input) return;
      const reveal = input.type === "password";
      input.type = reveal ? "text" : "password";
      $(this).attr("aria-label", reveal ? "Hide password" : "Show password");
      $(this).find("i").toggleClass("bi-eye", !reveal).toggleClass("bi-eye-slash", reveal);
    });

    $("#loginForm").on("submit", function (event) {
      event.preventDefault();
      const form = this;
      const $form = $(form);
      $form.addClass("was-validated");
      if (!form.checkValidity()) return;

      const $button = $("#loginButton").prop("disabled", true);
      $button.find(".button-label").text("Signing in…");
      $button.find(".spinner-border").removeClass("d-none");
      $("#loginError").addClass("d-none").text("");

      $.ajax({
        url: API_BASE_URL + "/login",
        method: "POST",
        contentType: "application/json",
        dataType: "json",
        data: JSON.stringify({
          email: $("#loginEmail").val().trim(),
          password: $("#loginPassword").val()
        })
      }).done(function (response) {
        if (!response || response.success !== true || !response.data ||
            typeof response.data.token !== "string" || !response.data.token ||
            typeof response.data.fullName !== "string") {
          $("#loginError").text("The API returned an invalid sign-in response.").removeClass("d-none");
          return;
        }
        sessionStorage.setItem(TOKEN_KEY, response.data.token);
        sessionStorage.setItem(USER_KEY, JSON.stringify({
          fullName: response.data.fullName,
          email: response.data.email
        }));
        window.location.replace("index.html");
      }).fail(function (xhr) {
        const message = xhr.responseJSON && xhr.responseJSON.message
          ? xhr.responseJSON.message
          : "Unable to sign in. Check your connection and try again.";
        $("#loginError").text(message).removeClass("d-none");
      }).always(function () {
        $button.prop("disabled", false);
        $button.find(".button-label").text("Sign in");
        $button.find(".spinner-border").addClass("d-none");
      });
    });

    $("#registerForm").on("submit", function (event) {
      event.preventDefault();
      const form = this;
      const password = $("#registerPassword").val();
      const confirmPassword = $("#confirmPassword").val();
      const confirmInput = document.getElementById("confirmPassword");
      confirmInput.setCustomValidity(password === confirmPassword ? "" : "Passwords do not match.");
      $(form).addClass("was-validated");
      if (!form.checkValidity()) return;

      const $button = $("#registerButton").prop("disabled", true);
      $button.find(".button-label").text("Creating account…");
      $button.find(".spinner-border").removeClass("d-none");
      $("#registerError").addClass("d-none").text("");
      $("#registerSuccess").addClass("d-none").text("");

      $.ajax({
        url: API_BASE_URL + "/register",
        method: "POST",
        contentType: "application/json",
        dataType: "json",
        data: JSON.stringify({
          fullName: $("#registerFullName").val().trim(),
          email: $("#registerEmail").val().trim(),
          password: password,
          confirmPassword: confirmPassword
        })
      }).done(function (response) {
        const message = response && response.message
          ? response.message
          : "Registration successful. You can now sign in.";
        $("#registerSuccess").text(message).removeClass("d-none");
        form.reset();
        $(form).removeClass("was-validated");
        window.setTimeout(function () {
          window.location.replace("login.html");
        }, 1400);
      }).fail(function (xhr) {
        const body = xhr.responseJSON;
        if (body && body.errors) {
          const fieldIds = {
            fullname: "registerFullName",
            email: "registerEmail",
            password: "registerPassword",
            confirmpassword: "confirmPassword"
          };
          Object.keys(body.errors).forEach(function (key) {
            const input = document.getElementById(fieldIds[key.toLowerCase()]);
            const error = body.errors[key][0];
            if (input && error) {
              input.setCustomValidity(error);
              $(input).siblings(".invalid-feedback").text(error);
              $(form).addClass("was-validated");
            }
          });
        }
        const message = body && body.message
          ? body.message
          : body && body.title
            ? body.title
            : body && body.errors
              ? "Please review the highlighted fields."
              : "Unable to create the account. Please try again.";
        $("#registerError").text(message).removeClass("d-none");
      }).always(function () {
        $button.prop("disabled", false);
        $button.find(".button-label").text("Create account");
        $button.find(".spinner-border").addClass("d-none");
      });
    });

    $("#registerForm input").on("input", function () {
      this.setCustomValidity("");
      if (this.id === "registerPassword" || this.id === "confirmPassword") {
        const password = $("#registerPassword").val();
        const confirmPassword = $("#confirmPassword").val();
        document.getElementById("confirmPassword").setCustomValidity(
          !confirmPassword || password === confirmPassword ? "" : "Passwords do not match."
        );
      }
    });
  });
})();
