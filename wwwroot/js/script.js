document.addEventListener("DOMContentLoaded", () => {
    const btn = document.querySelector(".burger");
    const drawer = document.getElementById("site-menu");
    const backdrop = document.querySelector(".menu-backdrop");
    const closeBtn = document.querySelector(".drawer-close");
    const topbar = document.querySelector(".topbar");

    if (!btn || !drawer) {
        console.warn("[KADYC] Falta .burger o #site-menu en el DOM.");
        return;
    }

    let isOpen = false;

    const openMenu = () => {
        isOpen = true;
        btn.classList.add("is-open");
        drawer.classList.add("is-open");
        drawer.setAttribute("aria-hidden", "false");
        btn.setAttribute("aria-expanded", "true");
        if (backdrop) {
            backdrop.hidden = false;
            // Añadir clase para animación de backdrop
            setTimeout(() => backdrop.classList.add('is-visible'), 10);
        }
        document.body.style.overflow = "hidden";

        // Actualizar badge del carrito en el drawer si está autenticado
        updateDrawerCartBadge();
    };

    const closeMenu = () => {
        isOpen = false;
        btn.classList.remove("is-open");
        drawer.classList.remove("is-open");
        drawer.setAttribute("aria-hidden", "true");
        btn.setAttribute("aria-expanded", "false");
        if (backdrop) {
            backdrop.classList.remove('is-visible');
            setTimeout(() => backdrop.hidden = true, 300);
        }
        document.body.style.overflow = "";
    };

    btn.addEventListener("click", () => (isOpen ? closeMenu() : openMenu()));
    if (backdrop) backdrop.addEventListener("click", closeMenu);
    if (closeBtn) closeBtn.addEventListener("click", closeMenu);

    // Cerrar drawer al hacer clic en un enlace de navegación
    const drawerLinks = drawer.querySelectorAll('a[data-link]');
    drawerLinks.forEach(link => {
        link.addEventListener('click', () => {
            if (!link.getAttribute('href').startsWith('#')) {
                closeMenu();
            }
        });
    });

    document.addEventListener("keydown", (e) => {
        if (e.key === "Escape" && isOpen) closeMenu();
    });

    // Función para actualizar el badge del carrito en el drawer
    function updateDrawerCartBadge() {
        const cartBadge = document.getElementById('cart-count-drawer-badge');
        if (cartBadge && window.updateCartCount) {
            // Intentar obtener el count actual del carrito
            fetch('/Cart/GetCartCount')
                .then(response => response.json())
                .then(count => {
                    cartBadge.textContent = count || '0';
                    if (count > 0) {
                        cartBadge.style.display = 'inline-flex';
                    } else {
                        cartBadge.style.display = 'none';
                    }
                })
                .catch(() => {
                    cartBadge.textContent = '0';
                    cartBadge.style.display = 'none';
                });
        }
    }


    // Función global para cerrar drawer y hacer scroll
    window.closeDrawerAndScroll = function (element) {
        const href = element.getAttribute('href');
        if (href && href.startsWith('#')) {
            closeMenu();
            setTimeout(() => scrollWithOffset(href), 300);
        }
    };

    // Smooth scroll con OFFSET (evita que "baje al final")
    function scrollWithOffset(hash) {
        if (!hash || !hash.startsWith("#")) return;
        const target = document.querySelector(hash);
        if (!target) return;

        const topbarH = topbar ? topbar.offsetHeight : 0;
        const rect = target.getBoundingClientRect();
        const absoluteTop = rect.top + window.scrollY;
        const finalTop = Math.max(0, absoluteTop - topbarH - 12);

        window.scrollTo({ top: finalTop, behavior: "smooth" });
    }

    // Mejorar la animación del backdrop
    const style = document.createElement('style');
    style.textContent = `
    .menu-backdrop.is-visible {
  opacity: 1 !important;
    }
    .drawer-nav a {
      opacity: 0;
      transform: translateX(-20px);
   transition: opacity 0.3s ease, transform 0.3s ease;
    }
    .drawer.is-open .drawer-nav a {
      opacity: 1;
      transform: translateX(0);
    }
  `;
    document.head.appendChild(style);
});

(function () { // Catálogo dinámico
    // init render
    if (document.getElementById('kad-grid')) render('all');
    document.querySelectorAll('.kad-chip').forEach(ch => {
        ch.addEventListener('click', () => {
            document.querySelectorAll('.kad-chip').forEach(c => c.classList.remove('is-active'));
            ch.classList.add('is-active');
            render(ch.dataset.filter);
        });
    });

    // Formulario validación
    const form = document.getElementById('kad-form');
    form && form.addEventListener('submit', function (e) {
        e.preventDefault();
        if (document.getElementById('kad-company').value) return;
        let ok = true;
        this.querySelectorAll('input[required], textarea[required]').forEach(el => {
            const err = el.parentElement.querySelector('.kad-error');
            if (!el.checkValidity()) { ok = false; err.textContent = el.validationMessage; }
            else err.textContent = '';
        });
        const status = document.getElementById('kad-status');
        if (ok) {
            status.textContent = '¡Gracias! Tu mensaje ha sido enviado.';
            console.log('Registro de envío (simulado):', {
                nombre: this.nombre.value, correo: this.correo.value, tel: this.tel.value, msg: this.msg.value, ts: new Date().toISOString()
            });
            this.reset();
        } else {
            status.textContent = 'Corrige los campos marcados.';
        }
    });
})();

// Mostrar el modal
document.getElementById('open-contact-form').addEventListener('click', function () {
    document.getElementById('kad-form-modal').style.display = 'flex';
    document.body.style.overflow = 'hidden';
});

// Ocultar el modal
document.getElementById('close-contact-form').addEventListener('click', function () {
    document.getElementById('kad-form-modal').style.display = 'none';
    document.body.style.overflow = '';
});

// Cerrar al hacer clic en el fondo
document.querySelector('.kad-form-modal__backdrop').addEventListener('click', function () {
    document.getElementById('kad-form-modal').style.display = 'none';
    document.body.style.overflow = '';
});

// Cerrar con Escape
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        document.getElementById('kad-form-modal').style.display = 'none';
        document.body.style.overflow = '';
    }
});