$(function () {
  const auth = window.EmployeeAuth;
  if (!auth || !auth.requireAuth()) return;

  const API_BASE_URL = "https://localhost:7039/api/Employee";
  const employeeModal = bootstrap.Modal.getOrCreateInstance("#employeeModal");
  const deleteModal = bootstrap.Modal.getOrCreateInstance("#deleteModal");
  const toast = bootstrap.Toast.getOrCreateInstance("#appToast", { delay: 3500 });
  const $form = $("#employeeForm");
  const $tbody = $("#employeeTableBody");
  let searchTimer;
  let searchRequest;
  let searchSequence = 0;
  let employeeToDelete = null;

  function showToast(title, message, isError) {
    $("#toastTitle").text(title);
    $("#toastMessage").text(message);
    $("#appToast").toggleClass("is-error", Boolean(isError));
    toast.show();
  }

  function responseMessage(xhr) {
    const body = xhr.responseJSON;
    if (body && body.message) return body.message;
    if (body && body.detail) return body.detail;
    if (body && body.errors) {
      const firstError = Object.values(body.errors).flat()[0];
      if (firstError) return firstError;
    }
    if (xhr.status === 0) return "Could not reach the API. Check that the API is running and accessible.";
    return "The request could not be completed. Please try again.";
  }

  function initials(name) {
    return name.trim().split(/\s+/).slice(0, 2).map(function (part) {
      return part.charAt(0).toUpperCase();
    }).join("");
  }

  function addCell($row, value) {
    return $("<td>").text(value || "—").appendTo($row);
  }

  function renderEmployees(employees, searchTerm) {
    $tbody.empty();
    $("#employeeCount").text(employees.length);

    if (employees.length === 0) {
      const isSearch = Boolean(searchTerm);
      $("#tableMessage")
        .html('<div class="empty-icon"><i class="bi ' + (isSearch ? "bi-search" : "bi-people") + '" aria-hidden="true"></i></div>' +
          '<div class="empty-title">' + (isSearch ? "No matching employees" : "No employees yet") + "</div>" +
          "<div>" + (isSearch ? "Try another name or email address." : "Add an employee to start building your directory.") + "</div>")
        .removeClass("d-none");
      return;
    }

    $("#tableMessage").addClass("d-none").empty();
    employees.forEach(function (employee) {
      const $row = $("<tr>");
      addCell($row, employee.employeeId);

      const $identity = $("<div>").addClass("employee-cell");
      $("<span>").addClass("employee-avatar").text(initials(employee.name || "")).appendTo($identity);
      const $details = $("<div>");
      $("<div>").addClass("employee-name").text(employee.name).appendTo($details);
      $("<div>").addClass("employee-email").text(employee.email).appendTo($details);
      $identity.append($details);
      $("<td>").append($identity).appendTo($row);

      addCell($row, employee.mobileNo);
      addCell($row, employee.department);
      addCell($row, employee.designation);

      const $actions = $("<div>").addClass("action-buttons");
      $("<button>", {
        type: "button",
        class: "action-button edit-employee",
        "aria-label": "Edit " + employee.name,
        title: "Edit employee"
      }).attr("data-id", employee.employeeId)
        .append($("<i>").addClass("bi bi-pencil").attr("aria-hidden", "true"))
        .appendTo($actions);
      $("<button>", {
        type: "button",
        class: "action-button delete delete-employee",
        "aria-label": "Delete " + employee.name,
        title: "Delete employee"
      }).attr("data-id", employee.employeeId)
        .append($("<i>").addClass("bi bi-trash3").attr("aria-hidden", "true"))
        .appendTo($actions);
      $("<td>").addClass("text-end").append($actions).appendTo($row);
      $tbody.append($row);
    });
  }

  function loadEmployees(searchTerm) {
    const requestNumber = ++searchSequence;
    if (searchRequest) searchRequest.abort();
    $("#tableLoading").removeClass("d-none");
    $("#tableMessage").addClass("d-none");
    $tbody.empty();

    searchRequest = $.ajax({
      url: API_BASE_URL,
      method: "GET",
      dataType: "json",
      data: searchTerm ? { search: searchTerm } : {}
    }).done(function (employees) {
      if (requestNumber === searchSequence) renderEmployees(employees, searchTerm);
    }).fail(function (xhr, status) {
      if (status === "abort" || requestNumber !== searchSequence) return;
      $tbody.empty();
      $("#employeeCount").text("—");
      $("#tableMessage").text(responseMessage(xhr)).removeClass("d-none");
    }).always(function () {
      if (requestNumber === searchSequence) $("#tableLoading").addClass("d-none");
    });
  }

  function setButtonLoading($button, loading) {
    $button.prop("disabled", loading);
    $button.find(".button-label").text(loading ? "Please wait…" : $button.data("default-label"));
    $button.find(".spinner-border").toggleClass("d-none", !loading);
  }

  function resetForm() {
    $form[0].reset();
    $form.removeClass("was-validated");
    $("#employeeId").val("");
    $("#formError").addClass("d-none").text("");
    $("#saveEmployeeButton").data("default-label", "Save employee");
    setButtonLoading($("#saveEmployeeButton"), false);
  }

  function populateForm(employee) {
    $("#employeeId").val(employee.employeeId);
    $("#employeeName").val(employee.name);
    $("#employeeEmail").val(employee.email);
    $("#employeeMobile").val(employee.mobileNo);
    $("#employeeDepartment").val(employee.department);
    $("#employeeDesignation").val(employee.designation);
  }

  $("#addEmployeeButton").on("click", function () {
    resetForm();
    $("#employeeModalTitle").text("Add employee");
    $("#saveEmployeeButton").data("default-label", "Save employee");
    employeeModal.show();
  });

  $form.on("submit", function (event) {
    event.preventDefault();
    $form.addClass("was-validated");
    if (!$form[0].checkValidity()) return;

    const id = $("#employeeId").val();
    const employee = {
      name: $("#employeeName").val().trim(),
      email: $("#employeeEmail").val().trim(),
      mobileNo: $("#employeeMobile").val().trim(),
      department: $("#employeeDepartment").val().trim(),
      designation: $("#employeeDesignation").val().trim()
    };
    const $button = $("#saveEmployeeButton");
    setButtonLoading($button, true);
    $("#formError").addClass("d-none").text("");

    $.ajax({
      url: id ? API_BASE_URL + "/" + encodeURIComponent(id) : API_BASE_URL,
      method: id ? "PUT" : "POST",
      contentType: "application/json",
      dataType: "json",
      data: JSON.stringify(employee)
    }).done(function () {
      employeeModal.hide();
      showToast("Success", id ? "Employee updated successfully." : "Employee added successfully.", false);
      loadEmployees($("#employeeSearch").val().trim());
    }).fail(function (xhr) {
      $("#formError").text(responseMessage(xhr)).removeClass("d-none");
    }).always(function () {
      setButtonLoading($button, false);
    });
  });

  $tbody.on("click", ".edit-employee", function () {
    const id = $(this).attr("data-id");
    $.ajax({
      url: API_BASE_URL + "/" + encodeURIComponent(id),
      method: "GET",
      dataType: "json"
    }).done(function (employee) {
      resetForm();
      populateForm(employee);
      $("#employeeModalTitle").text("Edit employee");
      $("#saveEmployeeButton").data("default-label", "Save changes");
      $("#saveEmployeeButton .button-label").text("Save changes");
      employeeModal.show();
    }).fail(function (xhr) {
      showToast("Unable to load employee", responseMessage(xhr), true);
    });
  });

  $tbody.on("click", ".delete-employee", function () {
    employeeToDelete = $(this).attr("data-id");
    $("#deleteError").addClass("d-none").text("");
    $("#confirmDeleteButton").data("default-label", "Delete employee");
    setButtonLoading($("#confirmDeleteButton"), false);
    deleteModal.show();
  });

  $("#confirmDeleteButton").on("click", function () {
    if (!employeeToDelete) return;
    const $button = $(this);
    setButtonLoading($button, true);
    $("#deleteError").addClass("d-none").text("");

    $.ajax({
      url: API_BASE_URL + "/" + encodeURIComponent(employeeToDelete),
      method: "DELETE",
      dataType: "json"
    }).done(function () {
      deleteModal.hide();
      showToast("Success", "Employee deleted successfully.", false);
      loadEmployees($("#employeeSearch").val().trim());
      employeeToDelete = null;
    }).fail(function (xhr) {
      $("#deleteError").text(responseMessage(xhr)).removeClass("d-none");
    }).always(function () {
      setButtonLoading($button, false);
    });
  });

  $("#employeeSearch").on("input", function () {
    const term = $(this).val().trim();
    window.clearTimeout(searchTimer);
    searchTimer = window.setTimeout(function () {
      loadEmployees(term);
    }, 300);
  });

  $("#employeeSearch").on("keydown", function (event) {
    if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "k") {
      event.preventDefault();
      $(this).trigger("focus");
    }
  });

  $("#employeeModal").on("hidden.bs.modal", resetForm);
  loadEmployees("");
});
