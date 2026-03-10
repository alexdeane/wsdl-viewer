function bindDropzone(zoneId) {
  var zone = document.getElementById(zoneId);
  if (!zone) return;

  var targetInputId = zone.getAttribute("data-target-input");
  if (!targetInputId) return;
  var fileInput = document.getElementById(targetInputId);
  if (!fileInput) return;

  function preventDefault(e) {
    e.preventDefault();
    e.stopPropagation();
  }

  ["dragenter", "dragover", "dragleave", "drop"].forEach(function (evt) {
    zone.addEventListener(evt, preventDefault, false);
  });

  ["dragenter", "dragover"].forEach(function (evt) {
    zone.addEventListener(evt, function () {
      if (zone.classList.contains("disabled")) return;
      zone.classList.add("active");
    });
  });

  ["dragleave", "drop"].forEach(function (evt) {
    zone.addEventListener(evt, function () {
      zone.classList.remove("active");
    });
  });

  zone.addEventListener("click", function () {
    if (zone.classList.contains("disabled")) return;
    fileInput.click();
  });

  zone.addEventListener("drop", function (e) {
    if (zone.classList.contains("disabled")) return;
    var files = e.dataTransfer && e.dataTransfer.files;
    if (!files || files.length === 0) return;
    fileInput.files = files;
    fileInput.dispatchEvent(new Event("change", { bubbles: true }));
  });
}

function attachClearButton(input) {
  var wrapper = input.closest(".input-with-clear");
  if (!wrapper) return;
  var btn = wrapper.querySelector(".clear-input-btn");
  if (!btn) return;

  function hasValue() {
    if (input.type === "file") {
      return input.files && input.files.length > 0;
    }
    return input.value && input.value.trim().length > 0;
  }

  function sync() {
    wrapper.classList.toggle("has-value", hasValue());
  }

  btn.addEventListener("click", function () {
    if (input.type === "file") {
      input.value = "";
    } else {
      input.value = "";
    }
    input.dispatchEvent(new Event("input", { bubbles: true }));
    input.dispatchEvent(new Event("change", { bubbles: true }));
    sync();
  });

  input.addEventListener("input", sync);
  input.addEventListener("change", sync);
  sync();
}

function setupMutualExclusion(group) {
  var uriInput = document.getElementById(group.uriId);
  var fileInput = document.getElementById(group.fileId);
  var dropzone = document.getElementById(group.dropzoneId);

  if (!uriInput || !fileInput) return;

  function updateState() {
    var uriHasValue = uriInput.value && uriInput.value.trim().length > 0;
    var fileHasValue = fileInput.files && fileInput.files.length > 0;

    if (uriHasValue) {
      fileInput.disabled = true;
      if (dropzone) dropzone.classList.add("disabled");
    } else if (!fileHasValue) {
      fileInput.disabled = false;
      if (dropzone) dropzone.classList.remove("disabled");
    }

    if (fileHasValue) {
      uriInput.readOnly = true;
      uriInput.disabled = true;
    } else if (!uriHasValue) {
      uriInput.readOnly = false;
      uriInput.disabled = false;
    }
  }

  uriInput.addEventListener("input", updateState);
  fileInput.addEventListener("change", updateState);

  updateState();
}

function initTooltips() {
  if (typeof bootstrap === "undefined" || !bootstrap.Tooltip) return;
  var tooltipTriggerList = [].slice.call(
    document.querySelectorAll('[data-bs-toggle="tooltip"]')
  );
  tooltipTriggerList.forEach(function (el) {
    new bootstrap.Tooltip(el);
  });
}

document.addEventListener("DOMContentLoaded", function () {
  bindDropzone("wsdl-dropzone");
  bindDropzone("xsd-dropzone");

  ["wsdl_uri", "wsdl_file", "xsd_uri", "xsd_file"].forEach(function (id) {
    var input = document.getElementById(id);
    if (input) attachClearButton(input);
  });

  setupMutualExclusion({
    uriId: "wsdl_uri",
    fileId: "wsdl_file",
    dropzoneId: "wsdl-dropzone"
  });
  setupMutualExclusion({
    uriId: "xsd_uri",
    fileId: "xsd_file",
    dropzoneId: "xsd-dropzone"
  });

  initTooltips();
});
