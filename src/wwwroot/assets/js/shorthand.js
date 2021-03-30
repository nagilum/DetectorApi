"use strict";

/**
 * Shorthand for document.createElement.
 * @param {String} tagName Name of the tag to create.
 * @returns {Element} Created element.
 */
const ce = (tagName) => {
    return document.createElement(tagName);
};

/**
 * Shorthand for document.querySelector.
 * @param {String} selector DOM selector.
 * @returns {Element} Selected element.
 */
const qs = (selector) => {
    return document.querySelector(selector);
};

/**
 * Shorthand for document.querySelectorAll.
 * @param {String} selector 
 * @returns {Array} Array of elements.
 */
const qsa = (selector) => {
    return document.querySelectorAll(selector);
};