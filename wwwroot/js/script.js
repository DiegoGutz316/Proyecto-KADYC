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