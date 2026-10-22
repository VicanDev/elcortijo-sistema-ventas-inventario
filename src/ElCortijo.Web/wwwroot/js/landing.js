(() => {
    const header = document.getElementById('header');
    const menuButton = document.getElementById('menuToggle');
    const menu = document.getElementById('landingNav');

    const syncHeader = () => header?.classList.toggle('scrolled', window.scrollY > 8);
    syncHeader();
    window.addEventListener('scroll', syncHeader, { passive: true });

    menuButton?.addEventListener('click', () => {
        const isOpen = menu?.classList.toggle('open') ?? false;
        menuButton.setAttribute('aria-expanded', String(isOpen));
        menuButton.innerHTML = `<i class="bi ${isOpen ? 'bi-x-lg' : 'bi-list'}"></i>`;
        document.body.classList.toggle('menu-open', isOpen);
    });

    menu?.querySelectorAll('a').forEach(link => {
        link.addEventListener('click', () => {
            menu.classList.remove('open');
            menuButton?.setAttribute('aria-expanded', 'false');
            if (menuButton) menuButton.innerHTML = '<i class="bi bi-list"></i>';
            document.body.classList.remove('menu-open');
        });
    });
})();
