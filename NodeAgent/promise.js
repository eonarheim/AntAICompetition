(function () {
    'use strict';
    /**
    * Promises/A+ spec implementation of promises
    * @class Promise
    * @constructor
    */
var Promise = (function () {
        function Promise() {
            this._state = 2 /* Pending */;
            this.successCallbacks = [];
            this.rejectCallback = function () {
            };
        }
        /**
         * Wrap a value in a resolved promise
         * @method wrap<T>
         * @param [value=undefined] {T} An optional value to wrap in a resolved promise
         * @returns Promise&lt;T&gt;
         */
        Promise.wrap = function (value) {
            var promise = (new Promise()).resolve(value);
            return promise;
        };
        /**
         * Returns a new promise that resolves when all the promises passed to it resolve, or rejects
         * when at least 1 promise rejects.
         * @param promises {Promise[]}
         * @returns Promise
         */
        Promise.join = function () {
            var promises = [];
            for (var _i = 0; _i < arguments.length; _i++) {
                promises[_i - 0] = arguments[_i];
            }
            var joinedPromise = new Promise();
            if (!promises) {
                return joinedPromise.resolve();
            }
            var total = promises.length;
            var successes = 0;
            var rejects = 0;
            var errors = [];
            promises.forEach(function (p) {
                p.then(function () {
                    successes += 1;
                    if (successes === total) {
                        joinedPromise.resolve();
                    }
                    else if (successes + rejects + errors.length === total) {
                        joinedPromise.reject(errors);
                    }
                }, function () {
                    rejects += 1;
                    if (successes + rejects + errors.length === total) {
                        joinedPromise.reject(errors);
                    }
                }).error(function (e) {
                    errors.push(e);
                    if ((errors.length + successes + rejects) === total) {
                        joinedPromise.reject(errors);
                    }
                });
            });
            return joinedPromise;
        };
        /**
         * Chain success and reject callbacks after the promise is resovled
         * @method then
         * @param successCallback {T=>any} Call on resolution of promise
         * @param rejectCallback {any=>any} Call on rejection of promise
         * @returns Promise&lt;T&gt;
         */
        Promise.prototype.then = function (successCallback, rejectCallback) {
            if (successCallback) {
                this.successCallbacks.push(successCallback);
                // If the promise is already resovled call immediately
                if (this.state() === 0/* Resolved */) {
                    try {
                        successCallback.call(this, this.value);
                    }
                    catch (e) {
                        this.handleError(e);
                    }
                }
            }
            if (rejectCallback) {
                this.rejectCallback = rejectCallback;
                // If the promise is already rejected call immediately
                if (this.state() === 1/* Rejected */) {
                    try {
                        rejectCallback.call(this, this.value);
                    }
                    catch (e) {
                        this.handleError(e);
                    }
                }
            }
            return this;
        };
        /**
         * Add an error callback to the promise
         * @method error
         * @param errorCallback {any=>any} Call if there was an error in a callback
         * @returns Promise&lt;T&gt;
         */
        Promise.prototype.error = function (errorCallback) {
            if (errorCallback) {
                this.errorCallback = errorCallback;
            }
            return this;
        };
        /**
         * Resolve the promise and pass an option value to the success callbacks
         * @method resolve
         * @param [value=undefined] {T} Value to pass to the success callbacks
         */
        Promise.prototype.resolve = function (value) {
            var _this = this;
            if (this._state === 2/* Pending */) {
                this.value = value;
                try {
                    this._state = 0 /* Resolved */;
                    this.successCallbacks.forEach(function (cb) {
                        cb.call(_this, _this.value);
                    });
                }
                catch (e) {
                    this.handleError(e);
                }
            }
            else {
                throw new Error('Cannot resolve a promise that is not in a pending state!');
            }
            return this;
        };
        /**
         * Reject the promise and pass an option value to the reject callbacks
         * @method reject
         * @param [value=undefined] {T} Value to pass to the reject callbacks
         */
        Promise.prototype.reject = function (value) {
            if (this._state === 2/* Pending */) {
                this.value = value;
                try {
                    this._state = 1 /* Rejected */;
                    this.rejectCallback.call(this, this.value);
                }
                catch (e) {
                    this.handleError(e);
                }
            }
            else {
                throw new Error('Cannot reject a promise that is not in a pending state!');
            }
            return this;
        };
        /**
         * Inpect the current state of a promise
         * @method state
         * @returns PromiseState
         */
        Promise.prototype.state = function () {
            return this._state;
        };
        Promise.prototype.handleError = function (e) {
            if (this.errorCallback) {
                this.errorCallback.call(this, e);
            }
            else {
                throw e;
            }
        };
        return Promise;
    })();
    
    module.exports = Promise;
})();