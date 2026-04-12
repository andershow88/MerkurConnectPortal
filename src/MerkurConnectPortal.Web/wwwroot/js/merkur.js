// MerkurConnect Portal – Client-seitige Hilfsfunktionen

// --- Dark Mode (wird sofort ausgeführt, vor DOMContentLoaded) ---
(function () {
    var saved = localStorage.getItem('mc-theme') || 'light';
    document.documentElement.setAttribute('data-theme', saved);
})();

function mcApplyTheme(theme) {
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('mc-theme', theme);
    var icon = document.getElementById('themeIcon');
    if (icon) {
        icon.className = theme === 'dark' ? 'bi bi-moon-stars-fill' : 'bi bi-sun-fill';
    }
}

document.addEventListener('DOMContentLoaded', function () {

    // --- Theme Toggle ---
    var themeToggle = document.getElementById('themeToggle');
    if (themeToggle) {
        // Icon beim Laden korrekt setzen
        var currentTheme = document.documentElement.getAttribute('data-theme') || 'light';
        mcApplyTheme(currentTheme);

        themeToggle.addEventListener('click', function () {
            var current = document.documentElement.getAttribute('data-theme') || 'light';
            mcApplyTheme(current === 'dark' ? 'light' : 'dark');
        });
    }

    // --- Sidebar Toggle (Mobile) ---
    const toggleBtn = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('sidebar');

    if (toggleBtn && sidebar) {
        toggleBtn.addEventListener('click', () => {
            sidebar.classList.toggle('open');
        });

        // Sidebar schließen wenn außerhalb geklickt
        document.addEventListener('click', (e) => {
            if (sidebar.classList.contains('open') &&
                !sidebar.contains(e.target) &&
                e.target !== toggleBtn) {
                sidebar.classList.remove('open');
            }
        });
    }

    // --- Suchfeld Auto-Submit mit Debounce ---
    const searchInput = document.getElementById('suchfeld');
    if (searchInput) {
        let debounceTimer;
        searchInput.addEventListener('input', function () {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(() => {
                const form = this.closest('form');
                if (form) form.submit();
            }, 600);
        });
    }

    // --- Tooltips initialisieren ---
    const tooltipEls = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    tooltipEls.forEach(el => new bootstrap.Tooltip(el));

    // --- Datei-Upload Vorschau ---
    const fileInput = document.getElementById('dateiInput');
    const fileLabel = document.getElementById('dateiLabel');
    if (fileInput && fileLabel) {
        fileInput.addEventListener('change', function () {
            if (this.files && this.files.length > 0) {
                const name = this.files[0].name;
                const size = formatBytes(this.files[0].size);
                fileLabel.textContent = `${name} (${size})`;
                fileLabel.classList.add('text-success');
            }
        });
    }

    // --- Fortschrittsbalken Animation ---
    document.querySelectorAll('.mc-progress-bar[data-width]').forEach(bar => {
        const width = bar.getAttribute('data-width');
        setTimeout(() => { bar.style.width = width + '%'; }, 100);
    });

    // --- Nachrichten-Scroll ---
    const messageList = document.querySelector('.mc-message-list');
    if (messageList) {
        messageList.scrollTop = messageList.scrollHeight;
    }
});

function formatBytes(bytes) {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
}
