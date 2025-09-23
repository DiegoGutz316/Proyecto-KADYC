document.addEventListener("DOMContentLoaded", () => {
  const btn = document.querySelector(".burger");
  const drawer = document.getElementById("site-menu");
  const backdrop = document.querySelector(".menu-backdrop"); // opcional
  const closeBtn = document.querySelector(".drawer-close"); // opcional
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
    if (backdrop) backdrop.hidden = false;
    document.body.style.overflow = "hidden";
  };

  const closeMenu = () => {
    isOpen = false;
    btn.classList.remove("is-open");
    drawer.classList.remove("is-open");
    drawer.setAttribute("aria-hidden", "true");
    btn.setAttribute("aria-expanded", "false");
    if (backdrop) backdrop.hidden = true;
    document.body.style.overflow = "";
  };

  btn.addEventListener("click", () => (isOpen ? closeMenu() : openMenu()));
  if (backdrop) backdrop.addEventListener("click", closeMenu);
  if (closeBtn) closeBtn.addEventListener("click", closeMenu);
  document.addEventListener("keydown", (e) => {
    if (e.key === "Escape" && isOpen) closeMenu();
  });

  // Smooth scroll con OFFSET (evita que “baje al final”)
  function scrollWithOffset(hash) {
    if (!hash || !hash.startsWith("#")) return;
    const target = document.querySelector(hash);
    if (!target) return;

    const topbarH = topbar ? topbar.offsetHeight : 0;
    const rect = target.getBoundingClientRect();
    const absoluteTop = rect.top + window.scrollY;
    const finalTop = Math.max(0, absoluteTop - topbarH - 12); // 12px de respiro

    window.scrollTo({ top: finalTop, behavior: "smooth" });
  }


});


(function () { // Catálogo dinámico
  if (!document.getElementById('catalogo')) {
    const sec = document.createElement('section');
    sec.id = 'catalogo';
    sec.className = 'kad-section';
    sec.innerHTML = '<div class="kad-wrap">\
      <header><h2>Destacados</h2><p>Selección curada (nuevo y segunda mano)</p></header>\
      <div class="kad-chipbar" role="region" aria-label="Filtros">\
        <button class="kad-chip is-active" data-filter="all">Todo</button>\
        <button class="kad-chip" data-filter="nuevo">Nuevo</button>\
        <button class="kad-chip" data-filter="segunda">Segunda mano</button>\
      </div>\
      <div id="kad-grid" class="kad-grid" role="list"></div>\
    </div>';
    const main = document.querySelector('main') || document.body;
    main.appendChild(sec);
  }
  const productos = [
    { id: 1, nombre: 'Camisa Oxford', precio: 19500, estado: 'nuevo', stock: true, img: 'https://i.pinimg.com/736x/15/d6/a2/15d6a2ecb078cc4079aac26201735815.jpg' },
    { id: 2, nombre: 'Pantalón Sastre', precio: 21000, estado: 'segunda', stock: true, img: 'https://i.pinimg.com/736x/8f/13/a1/8f13a1ccb433fe69d84fdd8c688c57c7.jpg' },
    { id: 3, nombre: 'Beige Knit Sweater', precio: 28000, estado: 'segunda', stock: false, img: 'https://i.pinimg.com/736x/35/b8/9e/35b89e779f62a0d2203d80e4cf577e4d.jpg' },
    { id: 4, nombre: 'Chaqueta Denim Oversize', precio: 22000, estado: 'nuevo', stock: true, img: 'https://i.pinimg.com/736x/f5/16/05/f51605b6735a8cc878f1c4a4ed7de43a.jpg' },
    { id: 5, nombre: 'Urban Black Set', precio: 24000, estado: 'nuevo', stock: true, img: 'https://i.pinimg.com/736x/ae/ed/09/aeed09279f73b09be460a24ad45e80d8.jpg' },
    { id: 6, nombre: 'Jeans Recto', precio: 24900, estado: 'segunda', stock: true, img: 'https://i.pinimg.com/736x/7c/35/e3/7c35e3ad05c89122790306652674b4d6.jpg' }
  ];
  function render(filter) {
    const grid = document.getElementById('kad-grid');
    if (!grid) return;
    grid.innerHTML = '';
    const list = productos.filter(p => filter === 'all' || !filter ? true : p.estado === filter);
    list.forEach(p => {
      const el = document.createElement('article');
      el.className = 'kad-card'; el.setAttribute('role', 'listitem');
      el.innerHTML = '<img src="' + p.img + '" alt="' + p.nombre + '" loading="lazy">\
        <div class="kad-card__body">\
          <strong>'+ p.nombre + '</strong>\
          <div><span class="kad-badge">'+ (p.estado === 'segunda' ? 'Segunda mano' : 'Nuevo') + '</span> \
               <span class="kad-badge">'+ (p.stock ? 'Disponible' : 'Agotado') + '</span></div>\
          <div><b>₡'+ p.precio.toLocaleString('es-CR') + '</b></div>\
          <div style="display:flex;gap:.5rem">\
            <button class="kad-btn kad-btn--solid">Detalles</button>\
            <a class="kad-btn kad-btn--ghost" href="#contacto">Comprar</a>\
          </div>\
        </div>';
      grid.appendChild(el);
    });
  }
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