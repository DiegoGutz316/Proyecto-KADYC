// Funcionalidad del carrito para Index.cshtml
document.addEventListener('DOMContentLoaded', function() {
    // Actualizar contador del carrito solo si el usuario está autenticado
    if (document.querySelector('[data-authenticated="true"]')) {
        updateCartCount();
    }
    
    // Actualizar el enlace de la cesta en la topbar
    updateCartLink();
});

function updateCartCount() {
    // Esta función se ejecuta solo para usuarios autenticados
    fetch('/Cart/GetCartCount')
        .then(response => response.json())
        .then(count => {
            // Actualizar contador en diferentes ubicaciones
            const cartElements = document.querySelectorAll('[id^="cart-count"]');
            cartElements.forEach(element => {
                if (element.id === 'cart-count') {
                    element.textContent = '[' + count + ']';
                } else if (element.id === 'cart-count-drawer') {
                    element.textContent = '[' + count + ']';
                }
            });
        })
        .catch(error => console.error('Error updating cart count:', error));
}

function updateCartLink() {
    const cestaLink = document.querySelector('a[href="#cesta"]');
    if (cestaLink) {
        // Determinar si el usuario está autenticado
        const isAuthenticated = document.querySelector('form[action*="Logout"]') !== null;
        
        if (isAuthenticated) {
            // Usuario autenticado: enlazar al carrito
            cestaLink.href = '/Cart';
            cestaLink.innerHTML = '<i class="fas fa-shopping-cart"></i> CESTA <span id="cart-count">[0]</span>';
            cestaLink.classList.add('cart-link');
        } else {
            // Usuario no autenticado: mostrar mensaje
            cestaLink.href = 'javascript:void(0)';
            cestaLink.onclick = showLoginRequired;
            cestaLink.innerHTML = '<i class="fas fa-shopping-cart"></i> CESTA [0]';
            cestaLink.classList.add('cart-link-disabled');
        }
    }
}

function showLoginRequired() {
    alert('Debes iniciar sesión para acceder al carrito de compras.');
    // Opcional: redirigir al login
    // window.location.href = '/Identity/Account/Login';
}

// Hacer disponible globalmente para otros scripts
window.updateCartCount = updateCartCount;