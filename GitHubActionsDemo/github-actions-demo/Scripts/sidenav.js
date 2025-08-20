let pageWrapper = document.getElementById('page-wrapper');
let sidebarButton = document.getElementById('sidebar-button');
let sidebarNavItems = document.querySelectorAll('.sidebar-nav-item');
let sidebarDropdownLink = document.querySelectorAll('.sidebar-dropdown-header-expand');

if (sidebarButton) {
    sidebarButton.onclick = () => {
        if (pageWrapper) {
            pageWrapper.classList.toggle('toggled');
        }
    };
}

let slideUp = (target, duration = 500) => {
    target.style.transitionProperty = 'height, margin, padding';
    target.style.transitionDuration = duration + 'ms';
    target.style.boxSizing = 'border-box';
    target.style.height = target.offsetHeight + 'px';
    target.offsetHeight;
    target.style.overflow = 'hidden';
    target.style.height = 0;
    target.style.paddingTop = 0;
    target.style.paddingBottom = 0;
    target.style.marginTop = 0;
    target.style.marginBottom = 0;
    window.setTimeout(() => {
        target.style.display = 'none';
        target.setAttribute('aria-hidden', 'true');
        target.style.removeProperty('height');
        target.style.removeProperty('padding-top');
        target.style.removeProperty('padding-bottom');
        target.style.removeProperty('margin-top');
        target.style.removeProperty('margin-bottom');
        target.style.removeProperty('overflow');
        target.style.removeProperty('transition-duration');
        target.style.removeProperty('transition-property');
    }, duration);
};

let slideDown = (target, duration = 500) => {
    target.style.removeProperty('display');
    let display = window.getComputedStyle(target).display;
    if (display === 'none') display = 'block';
    target.style.display = display;
    target.setAttribute('aria-hidden', 'false');
    let height = target.offsetHeight;
    target.style.overflow = 'hidden';
    target.style.height = 0;
    target.style.paddingTop = 0;
    target.style.paddingBottom = 0;
    target.style.marginTop = 0;
    target.style.marginBottom = 0;
    target.offsetHeight;
    target.style.boxSizing = 'border-box';
    target.style.transitionProperty = 'height, margin, padding';
    target.style.transitionDuration = duration + 'ms';
    target.style.height = height + 'px';
    target.style.removeProperty('padding-top');
    target.style.removeProperty('padding-bottom');
    target.style.removeProperty('margin-top');
    target.style.removeProperty('margin-bottom');
    window.setTimeout(() => {
        target.style.removeProperty('height');
        target.style.removeProperty('overflow');
        target.style.removeProperty('transition-duration');
        target.style.removeProperty('transition-property');
    }, duration);
};

sidebarDropdownLink.forEach((dropdownItem) => {
    dropdownItem.onclick = (event) => {
        let sidebarSubMenu = document.querySelectorAll('.sidebar-submenu');
        let expand = event.target;
        if (expand.nodeName !== 'A') {
            expand = expand.parentNode;
        }
        let header = expand.parentNode;

        sidebarSubMenu.forEach((subMenu) => {
            let display = window.getComputedStyle(subMenu).display;
            if (display === 'block') {
                slideUp(subMenu, 200);
            }
        });

        let wasActive = expand.classList.contains('active');

        sidebarDropdownLink.forEach((link) => {
            link.classList.remove('active');
        });

        if (!wasActive) {
            expand.classList.add('active');
        }

        let nextSibling = header.nextElementSibling;

        while (nextSibling && !nextSibling.classList.contains('sidebar-submenu')) {
            nextSibling = nextSibling.nextSibling;
        }
        slideDown(nextSibling, 200);
    };
});

sidebarNavItems.forEach((navItem) => {
    let url = window.location.href;
    if (navItem.href === url) {
        if (navItem.nextElementSibling && navItem.nextElementSibling.classList.contains('sidebar-dropdown-header-expand')) {
            navItem.nextElementSibling.classList.add('active');
            let parentNode = navItem.parentNode

            if (parentNode && parentNode.nextElementSibling.classList.contains('sidebar-submenu')) {
                slideDown(parentNode.nextElementSibling, 0);
            }
        }
        else {
            navItem.childNodes.forEach((child) => {
                if (child.nodeName === 'SPAN' && child.classList.contains('icon')) {
                    child.classList.remove('invisible');
                    let parentNode = navItem.parentNode;

                    while (parentNode && !parentNode.classList.contains('sidebar-submenu')) {
                        parentNode = parentNode.parentNode;
                    }

                    slideDown(parentNode, 0);

                    parentNode.previousElementSibling.childNodes.forEach((child) => {
                        if (child.nodeName === 'A' && child.classList.contains('sidebar-dropdown-header-expand')) {
                            child.classList.add('active');
                        }
                    });
                }
            });
        }
    }
});
