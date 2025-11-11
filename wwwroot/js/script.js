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
})();

// ============================================
//       FORMULARIO DE CONTACTO MEJORADO
// ============================================

class ContactFormHandler {
  constructor() {
        this.modal = document.getElementById('kad-form-modal');
        this.form = document.getElementById('kad-form');
        this.status = document.getElementById('kad-status');
        this.openBtn = document.getElementById('open-contact-form');
        this.closeBtn = document.getElementById('close-contact-form');
        this.backdrop = document.querySelector('.kad-form-modal__backdrop');

        this.isSubmitting = false;
        this.init();
    }

    init() {
   if (!this.form || !this.modal) return;

     // Eventos para abrir/cerrar modal
        this.openBtn?.addEventListener('click', () => this.openModal());
    this.closeBtn?.addEventListener('click', () => this.closeModal());
 this.backdrop?.addEventListener('click', () => this.closeModal());
  
      // Cerrar con Escape
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.isModalOpen()) {
                this.closeModal();
   }
        });

  // Manejar envío del formulario
        this.form.addEventListener('submit', (e) => this.handleSubmit(e));

        // Validación en tiempo real
     this.setupRealTimeValidation();

        // Mejorar accesibilidad
        this.setupAccessibility();
    }

    openModal() {
        this.modal.style.display = 'flex';
        this.modal.classList.add('kad-form-modal--open');
        document.body.style.overflow = 'hidden';
        
        // Focus en el primer campo
        setTimeout(() => {
     const firstInput = this.form.querySelector('input[name="nombre"]');
      if (firstInput) firstInput.focus();
        }, 100);

    // Limpiar mensajes previos
     this.clearStatus();
        this.clearErrors();
    }

    closeModal() {
     this.modal.classList.remove('kad-form-modal--open');
        document.body.style.overflow = '';
        
        setTimeout(() => {
        this.modal.style.display = 'none';
 }, 300);
    }

    isModalOpen() {
        return this.modal.classList.contains('kad-form-modal--open');
    }

    async handleSubmit(e) {
        e.preventDefault();
        
 if (this.isSubmitting) return;

        // Verificar honeypot
        const honeypot = this.form.querySelector('input[name="company"]');
        if (honeypot && honeypot.value.trim() !== '') {
   this.showError('Error en el envío. Por favor, inténtalo nuevamente.');
    return;
        }

        const formData = new FormData(this.form);
        const data = {
          nombre: formData.get('nombre'),
            correo: formData.get('correo'),
        telefono: formData.get('tel'),
      mensaje: formData.get('msg'),
    company: formData.get('company') || ''
        };

        // Validación local
        if (!this.validateForm(data)) {
   this.shakeModal();
     return;
        }

        try {
   this.setSubmitting(true);
          this.showStatus('Enviando mensaje...', 'info');

     const response = await fetch('/api/Contact/send', {
 method: 'POST',
    headers: {
     'Content-Type': 'application/json',
    },
             body: JSON.stringify(data)
});

    const result = await response.json();

 if (response.ok && result.success) {
  this.showSuccess(result.message);
       this.resetForm();
             
// Cerrar modal después de 3 segundos
   setTimeout(() => {
    this.closeModal();
         }, 3000);
         } else {
  this.handleFormErrors(result);
    }
        } catch (error) {
            console.error('Error al enviar formulario:', error);
  this.showError('Error de conexión. Verifica tu internet e inténtalo nuevamente.');
  } finally {
    this.setSubmitting(false);
        }
  }

    validateForm(data) {
        this.clearErrors();
 let isValid = true;

// Validar nombre
     if (!data.nombre || data.nombre.length < 2) {
      this.showFieldError('nombre', 'El nombre debe tener al menos 2 caracteres');
isValid = false;
        }

      // Validar email
 const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
     if (!data.correo || !emailRegex.test(data.correo)) {
     this.showFieldError('correo', 'Ingresa un correo electrónico válido');
      isValid = false;
        }

        // Validar teléfono
        const phoneRegex = /^[\+]?[\d\s\-\(\)]{7,20}$/;
        if (!data.telefono || !phoneRegex.test(data.telefono)) {
this.showFieldError('tel', 'Ingresa un número de teléfono válido');
            isValid = false;
     }

        // Validar mensaje
        if (!data.mensaje || data.mensaje.length < 10) {
      this.showFieldError('msg', 'El mensaje debe tener al menos 10 caracteres');
  isValid = false;
        }

        return isValid;
}

    showFieldError(fieldName, message) {
        const field = this.form.querySelector(`[name="${fieldName}"]`);
        const errorElement = field?.parentElement.querySelector('.kad-error');
        
        if (field && errorElement) {
            field.classList.add('error');
            field.parentElement.classList.add('kad-field--error');
    errorElement.textContent = message;
            errorElement.style.opacity = '1';
  errorElement.style.transform = 'translateY(0)';
 }
    }

    clearErrors() {
        this.form.querySelectorAll('.error').forEach(field => {
     field.classList.remove('error');
    field.parentElement.classList.remove('kad-field--error');
 });
        
        this.form.querySelectorAll('.kad-error').forEach(error => {
 error.textContent = '';
 error.style.opacity = '0';
 error.style.transform = 'translateY(-10px)';
     });
    }

    handleFormErrors(result) {
        if (result.errors && Object.keys(result.errors).length > 0) {
            // Mapear nombres de campos del backend a frontend
    const fieldMapping = {
      'Nombre': 'nombre',
      'Correo': 'correo',
     'Telefono': 'tel',
          'Mensaje': 'msg'
         };

            Object.entries(result.errors).forEach(([key, message]) => {
        const frontendFieldName = fieldMapping[key] || key.toLowerCase();
      this.showFieldError(frontendFieldName, message);
            });
        } else {
      this.showError(result.message || 'Error al enviar el formulario');
        }
    }

    showStatus(message, type = 'info') {
        if (!this.status) return;
   
        this.status.textContent = message;
        this.status.className = `kad-status kad-status--${type}`;
        this.status.style.opacity = '1';
        this.status.style.transform = 'translateY(0)';
    }

    showSuccess(message) {
        this.showStatus(message, 'success');
  }

    showError(message) {
        this.showStatus(message, 'error');
    }

    clearStatus() {
        if (this.status) {
 this.status.style.opacity = '0';
   this.status.style.transform = 'translateY(-10px)';
        }
    }

    setSubmitting(isSubmitting) {
   this.isSubmitting = isSubmitting;
        const submitBtn = this.form.querySelector('button[type="submit"]');
        
        if (submitBtn) {
submitBtn.disabled = isSubmitting;
      if (isSubmitting) {
          submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Enviando...';
         submitBtn.classList.add('kad-btn--loading');
     } else {
 submitBtn.innerHTML = 'Enviar mensaje';
       submitBtn.classList.remove('kad-btn--loading');
   }
        }
}

    resetForm() {
        this.form.reset();
   this.clearErrors();
    }

    shakeModal() {
      this.modal.querySelector('.kad-form-modal__content').classList.add('kad-modal--shake');
  setTimeout(() => {
            this.modal.querySelector('.kad-form-modal__content').classList.remove('kad-modal--shake');
        }, 600);
    }

    setupRealTimeValidation() {
     const inputs = this.form.querySelectorAll('input, textarea');
        
    inputs.forEach(input => {
    input.addEventListener('blur', () => {
         if (input.value.trim() && input.parentElement.classList.contains('kad-field--error')) {
         // Re-validar el campo si tenía error
        const data = {
 nombre: this.form.querySelector('[name="nombre"]').value,
             correo: this.form.querySelector('[name="correo"]').value,
          telefono: this.form.querySelector('[name="tel"]').value,
   mensaje: this.form.querySelector('[name="msg"]').value
           };
  
                 this.validateSingleField(input.name, data);
     }
      });

          input.addEventListener('input', () => {
    // Limpiar error mientras el usuario escribe
        if (input.parentElement.classList.contains('kad-field--error')) {
   const errorElement = input.parentElement.querySelector('.kad-error');
     if (errorElement && input.value.trim()) {
           input.classList.remove('error');
        input.parentElement.classList.remove('kad-field--error');
           errorElement.style.opacity = '0';
        }
    }
            });
        });
    }

    validateSingleField(fieldName, data) {
        const fieldMap = {
    'nombre': () => data.nombre && data.nombre.length >= 2,
  'correo': () => data.correo && /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(data.correo),
            'tel': () => data.telefono && /^[\+]?[\d\s\-\(\)]{7,20}$/.test(data.telefono),
            'msg': () => data.mensaje && data.mensaje.length >= 10
        };

        const isValid = fieldMap[fieldName]?.();
    if (isValid) {
            const field = this.form.querySelector(`[name="${fieldName}"]`);
          field.classList.remove('error');
            field.parentElement.classList.remove('kad-field--error');
         const errorElement = field.parentElement.querySelector('.kad-error');
  if (errorElement) {
         errorElement.style.opacity = '0';
     }
        }
    }

    setupAccessibility() {
     // Mejoras de accesibilidad
    this.modal.setAttribute('role', 'dialog');
        this.modal.setAttribute('aria-labelledby', 'contact-form-title');
      this.modal.setAttribute('aria-modal', 'true');
     
        // Manejar el focus trap
        this.setupFocusTrap();
    }

    setupFocusTrap() {
        const focusableElements = this.modal.querySelectorAll(
            'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
        );
  
        if (focusableElements.length === 0) return;
        
        const firstElement = focusableElements[0];
        const lastElement = focusableElements[focusableElements.length - 1];

        this.modal.addEventListener('keydown', (e) => {
  if (e.key === 'Tab') {
   if (e.shiftKey) {
            if (document.activeElement === firstElement) {
        lastElement.focus();
     e.preventDefault();
      }
            } else {
    if (document.activeElement === lastElement) {
     firstElement.focus();
        e.preventDefault();
     }
        }
            }
        });
    }
}

// Inicializar el manejador del formulario cuando el DOM esté listo
document.addEventListener('DOMContentLoaded', () => {
    window.contactFormHandler = new ContactFormHandler();
});