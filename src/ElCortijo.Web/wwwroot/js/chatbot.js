(() => {
    const root = document.querySelector('[data-chatbot]');
    if (!root) return;

    const panel = root.querySelector('[data-chat-panel]');
    const toggle = root.querySelector('[data-chat-toggle]');
    const close = root.querySelector('[data-chat-close]');
    const form = root.querySelector('[data-chat-form]');
    const input = root.querySelector('[data-chat-input]');
    const messages = root.querySelector('[data-chat-messages]');
    const suggestions = root.querySelector('[data-chat-suggestions]');
    const catalogButton = root.querySelector('[data-chat-catalog]');
    const stockButton = root.querySelector('[data-chat-stock]');
    const brandLabel = root.querySelector('[data-chat-brand-label]');
    const stopWords = new Set([
        'cuanto', 'cuantos', 'cuesta', 'cuestan', 'precio', 'precios', 'stock',
        'hay', 'tienen', 'tiene', 'disponible', 'disponibilidad', 'quiero',
        'quisiera', 'saber', 'consultar', 'consulta', 'de', 'del', 'la', 'el',
        'los', 'las', 'un', 'una', 'por', 'favor', 'para', 'producto',
        'productos', 'alimento', 'alimentos', 'comida', 'me', 'puedes',
        'decirme', 'dime', 'busco', 'buscar', 'ver', 'si', 'que', 'es',
        'mascota', 'mascotas', 'perro', 'perros', 'gato', 'gatos'
    ]);
    let selectedMode = '';

    const setOpen = open => {
        if (!panel || !toggle) return;
        panel.hidden = !open;
        toggle.setAttribute('aria-expanded', String(open));
        if (open) input.focus();
    };

    const addMessage = (text, kind = 'bot') => {
        const item = document.createElement('div');
        item.className = `cortijo-chat__message cortijo-chat__message--${kind}`;
        item.textContent = text;
        messages.appendChild(item);
        messages.scrollTop = messages.scrollHeight;
        return item;
    };

    const productTerm = question => {
        const words = question.normalize('NFD').replace(/[\u0300-\u036f]/g, '')
            .match(/[\p{L}\p{N}-]+/gu) || [];
        return words.filter(word => !stopWords.has(word.toLowerCase())).join(' ').trim();
    };

    const addProducts = (container, products) => {
        products.forEach(product => {
            const card = document.createElement('span');
            card.className = 'cortijo-chat__product';
            const name = document.createElement('strong');
            name.textContent = product.nombre;
            const code = document.createElement('small');
            code.textContent = `${product.marca ? `${product.marca} · ` : ''}${product.codigo} · ${product.unidadMedida}`;
            const price = document.createElement('small');
            const amount = Number(product.precio).toLocaleString('es-PE', { style: 'currency', currency: 'PEN' });
            const priceLabel = document.createElement('b');
            priceLabel.textContent = amount;
            price.append('Precio: ', priceLabel);
            const stock = document.createElement('span');
            stock.className = `cortijo-chat__stock${product.disponible ? '' : ' is-empty'}`;
            stock.textContent = product.disponible ? `Disponible · ${product.stock} en stock` : 'Agotado';
            card.append(name, code, price, stock);
            container.appendChild(card);
        });
    };

    const ask = async (question, showAll = false) => {
        const term = showAll ? '' : productTerm(question);
        addMessage(question, 'user');
        if (!term && !showAll) {
            addMessage('Escribe el nombre o código del alimento, por ejemplo “Canbo Adulto” o “PROD-005”.');
            return;
        }

        const loading = addMessage('Consultando el catálogo…');
        const send = form.querySelector('button[type="submit"]');
        send.disabled = true;
        try {
            const query = term ? `?buscar=${encodeURIComponent(term)}` : '';
            const response = await fetch(`/api/chatbot/productos${query}`, {
                headers: { 'Accept': 'application/json' }
            });
            const data = await response.json();
            loading.remove();
            if (!response.ok) {
                addMessage(data.mensaje || 'No pude consultar el catálogo. Intenta nuevamente.', 'error');
            } else if (!data.productos?.length) {
                addMessage(`No encontré “${term}”. Prueba con el código del producto o una parte más corta del nombre.`);
            } else {
                const reply = document.createElement('div');
                reply.className = 'cortijo-chat__message cortijo-chat__message--bot';
                const intro = document.createElement('span');
                intro.textContent = selectedMode === 'stock'
                    ? `Resultado de stock para ${term}:`
                    : showAll
                        ? `Estos son los ${data.productos.length} productos activos:`
                        : data.productos.length === 1 ? 'Encontré este producto:' : `Encontré ${data.productos.length} productos:`;
                reply.appendChild(intro);
                addProducts(reply, data.productos);
                messages.appendChild(reply);
                messages.scrollTop = messages.scrollHeight;
            }
        } catch {
            loading.remove();
            addMessage('No pude conectar con el catálogo. Revisa que el sistema esté iniciado e inténtalo otra vez.', 'error');
        } finally {
            send.disabled = false;
            input.focus();
        }
    };

    toggle?.addEventListener('click', () => setOpen(panel.hidden));
    close?.addEventListener('click', () => setOpen(false));
    const revealBrands = mode => {
        selectedMode = mode;
        suggestions.hidden = false;
        brandLabel.textContent = mode === 'stock' ? 'Elige una marca para revisar su stock' : 'Elige una marca para ver sus productos';
        addMessage(mode === 'stock'
            ? 'Perfecto. Selecciona una marca o escribe el nombre/código del producto para revisar existencias.'
            : 'Claro. Selecciona una marca para ver los productos de su catálogo.');
    };
    catalogButton?.addEventListener('click', () => revealBrands('products'));
    stockButton?.addEventListener('click', () => revealBrands('stock'));
    form.addEventListener('submit', event => {
        event.preventDefault();
        const question = input.value.trim();
        if (!question) return;
        input.value = '';
        ask(question);
    });
    suggestions.addEventListener('click', event => {
        const button = event.target.closest('[data-chat-query]');
        if (button) ask(button.dataset.chatQuery);
    });
    fetch('/api/chatbot/marcas', { headers: { 'Accept': 'application/json' } })
        .then(response => response.ok ? response.json() : Promise.reject())
        .then(data => {
            suggestions.querySelector('.cortijo-assistant-brands-loading')?.remove();
            (data.marcas || []).forEach(marca => {
                const button = document.createElement('button');
                button.type = 'button';
                button.dataset.chatQuery = marca;
                button.textContent = marca;
                suggestions.appendChild(button);
            });
        })
        .catch(() => {
            const loading = suggestions.querySelector('.cortijo-assistant-brands-loading');
            if (loading) loading.textContent = 'No se pudieron cargar las marcas';
        });
    document.addEventListener('keydown', event => {
        if (event.key === 'Escape' && panel && !panel.hidden) setOpen(false);
    });
})();
