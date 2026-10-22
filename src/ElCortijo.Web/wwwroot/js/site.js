(() => {
    const toggle = document.getElementById('themeToggle');
    if (!toggle) return;

    const applyTheme = theme => {
        const isDark = theme === 'dark';
        document.documentElement.setAttribute('data-bs-theme', isDark ? 'dark' : 'light');
        toggle.innerHTML = `<i class="bi ${isDark ? 'bi-sun-fill' : 'bi-moon-stars-fill'}" aria-hidden="true"></i>`;
        const label = isDark ? 'Activar modo claro' : 'Activar modo oscuro';
        toggle.setAttribute('aria-label', label);
        toggle.title = label;
    };

    applyTheme(localStorage.getItem('elcortijo-theme') || 'light');
    toggle.addEventListener('click', () => {
        const nextTheme = document.documentElement.getAttribute('data-bs-theme') === 'dark' ? 'light' : 'dark';
        localStorage.setItem('elcortijo-theme', nextTheme);
        applyTheme(nextTheme);
    });
})();
