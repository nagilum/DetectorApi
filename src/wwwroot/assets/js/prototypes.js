"use strict";

/**
 * Extender function for Element.querySelector.
 * @param {String} selector DOM selector.
 * @returns {Element} Selected element.
 */
 Element.prototype.qs = function (selector) {
    return this.querySelector(selector);
};

/**
 * Extender function for Element.querySelectorAll.
 * @param {String} selector 
 * @returns {Array} Array of elements.
 */
Element.prototype.qsa = function (selector) {
    return this.querySelectorAll(selector);
}