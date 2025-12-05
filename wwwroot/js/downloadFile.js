// wwwroot/js/downloadFile.js
window.downloadFile = function (url) {
    var link = document.createElement('a');
    link.href = url;
    link.download = '';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};
