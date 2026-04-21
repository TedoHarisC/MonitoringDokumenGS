/*
 * DataTables Buttons CDN loader for dashboard-home.js
 * This file is loaded dynamically if not present.
 */

(function() {
  if (!$.fn.DataTable) return;
  if ($.fn.dataTable.Buttons && window.JSZip) return;

  // 1. Load JSZip
  var jszipScript = document.createElement('script');
  jszipScript.src = 'https://cdnjs.cloudflare.com/ajax/libs/jszip/3.10.1/jszip.min.js';
  jszipScript.onload = function() {
    // 2. Load DataTables Buttons
    var btnScript = document.createElement('script');
    btnScript.src = 'https://cdn.datatables.net/buttons/2.4.2/js/dataTables.buttons.min.js';
    btnScript.onload = function() {
      // 3. Load HTML5 export
      var html5Script = document.createElement('script');
      html5Script.src = 'https://cdn.datatables.net/buttons/2.4.2/js/buttons.html5.min.js';
      document.head.appendChild(html5Script);
      // 4. Load CSS
      var link = document.createElement('link');
      link.rel = 'stylesheet';
      link.href = 'https://cdn.datatables.net/buttons/2.4.2/css/buttons.dataTables.min.css';
      document.head.appendChild(link);
    };
    document.head.appendChild(btnScript);
  };
  document.head.appendChild(jszipScript);
})();
