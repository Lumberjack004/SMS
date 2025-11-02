// JavaScript helper functions for SMS platform

// Download file helper
function downloadFile(fileName, contentType, byteArray) {
    const blob = new Blob([byteArray], { type: contentType });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
}

// Show success message
function showSuccess(message) {
    // Simple alert for now, can be enhanced with better UI
    alert('成功: ' + message);
}

// Show error message
function showError(message) {
    // Simple alert for now, can be enhanced with better UI
    alert('错误: ' + message);
}

// Confirm dialog helper
function confirmDialog(message) {
    return confirm(message);
}
