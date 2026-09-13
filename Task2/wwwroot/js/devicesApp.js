var app = angular.module("devicesApp", ["ui.bootstrap"]);

/* ========================= Directives ========================= */

app.directive("clientTypeSelector", function () {
    return {
        restrict: "E",
        scope: {
            selectedValue: "=",
            includeAll: "=",
            cssClass: "@",
            onChanged: "&?"
        },
        template:
            '<select ng-class="cssClass" ' +
            '        ng-model="selectedValue" ' +
            '        ng-options="type.value as type.name for type in clientTypes" ' +
            '        ng-change="notifyChange()">' +
            '   <option value="" ng-if="includeAll">All Clients</option>' +
            '   <option value="" ng-if="!includeAll">Select client type</option>' +
            '</select>',
        link: function (scope) {
            scope.clientTypes = [
                { value: 1, name: "Individual" },
                { value: 2, name: "Organization" }
            ];

            scope.notifyChange = function () {
                if (scope.onChanged) {
                    scope.onChanged({
                        selectedValue: scope.selectedValue
                    });
                }
            };
        }
    };
});

app.directive("deviceSelector", function () {
    return {
        restrict: "E",
        scope: {
            selectedValue: "=",
            devices: "=",
            includeAll: "=",
            cssClass: "@",
            onChanged: "&?"
        },
        template:
            '<select ng-class="cssClass" ' +
            '        ng-model="selectedValue" ' +
            '        ng-options="device.id as device.name for device in devices" ' +
            '        ng-change="notifyChange()">' +
            '   <option value="" ng-if="includeAll">All Devices</option>' +
            '   <option value="" ng-if="!includeAll">Select device</option>' +
            '</select>',
        link: function (scope) {
            scope.notifyChange = function () {
                if (scope.onChanged) {
                    scope.onChanged({
                        selectedValue: scope.selectedValue
                    });
                }
            };
        }
    };
});

app.directive("phoneNumberSelector", function () {
    return {
        restrict: "E",
        scope: {
            selectedValue: "=",
            phoneNumbers: "=",
            includeAll: "=",
            cssClass: "@",
            placeholder: "@",
            onChanged: "&?"
        },
        template:
            '<select ng-class="cssClass" ' +
            '        ng-model="selectedValue" ' +
            '        ng-options="phoneNumber.id as phoneNumber.number for phoneNumber in phoneNumbers" ' +
            '        ng-change="notifyChange()">' +
            '   <option value="" ng-if="includeAll">{{ placeholder || "All Phone Numbers" }}</option>' +
            '   <option value="" ng-if="!includeAll">{{ placeholder || "Select phone number" }}</option>' +
            '</select>',
        link: function (scope) {
            scope.notifyChange = function () {
                if (scope.onChanged) {
                    scope.onChanged({
                        selectedValue: scope.selectedValue
                    });
                }
            };
        }
    };
});

app.directive("clientSelector", function () {
    return {
        restrict: "E",
        scope: {
            selectedValue: "=",
            clients: "=",
            includeAll: "=",
            cssClass: "@",
            placeholder: "@",
            onChanged: "&?"
        },
        template:
            '<select ng-class="cssClass" ' +
            '        ng-model="selectedValue" ' +
            '        ng-options="client.id as client.name for client in clients" ' +
            '        ng-change="notifyChange()">' +
            '   <option value="" ng-if="includeAll">{{ placeholder || "All Clients" }}</option>' +
            '   <option value="" ng-if="!includeAll">{{ placeholder || "Select client" }}</option>' +
            '</select>',
        link: function (scope) {
            scope.notifyChange = function () {
                if (scope.onChanged) {
                    scope.onChanged({
                        selectedValue: scope.selectedValue
                    });
                }
            };
        }
    };
});

app.directive("reservationStatusSelector", function () {
    return {
        restrict: "E",
        scope: {
            selectedValue: "=",
            cssClass: "@",
            onChanged: "&?"
        },
        template:
            '<select ng-class="cssClass" ' +
            '        ng-model="selectedValue" ' +
            '        ng-change="notifyChange()">' +
            '   <option value="">All Status</option>' +
            '   <option value="active">Active</option>' +
            '   <option value="ended">Ended</option>' +
            '</select>',
        link: function (scope) {
            scope.notifyChange = function () {
                if (scope.onChanged) {
                    scope.onChanged({
                        selectedValue: scope.selectedValue
                    });
                }
            };
        }
    };
});

app.directive("fileModel", function () {
    return {
        restrict: "A",
        scope: {
            fileModel: "="
        },
        link: function (scope, element) {
            element.bind("change", function () {
                scope.$apply(function () {
                    scope.fileModel = element[0].files[0];
                });
            });
        }
    };
});

app.controller("MyRequestsController", function ($scope, $http, $timeout) {

    $scope.requests = [];
    $scope.statusFilter = "";
    $scope.isLoading = false;
    $scope.loadingMessage = "Loading your requests...";

    $scope.toast = {
        show: false,
        type: "success",
        title: "",
        message: ""
    };

    function showToast(type, title, message) {
        $scope.toast = {
            show: true,
            type: type,
            title: title,
            message: message
        };

        $timeout(function () {
            $scope.toast.show = false;
        }, 2500);
    }

    $scope.loadMyRequests = function () {
        $scope.isLoading = true;
        $scope.loadingMessage = "Loading your requests...";

        $http.get("/api/myRequests/GetMyRequests", {
            params: {
                status: $scope.statusFilter
            }
        })
            .then(function (response) {
                $scope.requests = response.data;
            })
            .catch(function (error) {
                var message = "Failed to load your requests.";

                if (error.data) {
                    message = error.data;
                }

                showToast("error", "Error", message);
            })
          
            .finally(function () {
                $scope.isLoading = false;
            });
    };

    $scope.loadMyRequests();
});

/* ========================= Devices Page ========================= */

app.controller("DevicesController", function ($scope, $http, $uibModal, $timeout) {

    $scope.devices = [];
    $scope.categories = [];

    $scope.dashboard = {
        totalDevices: 0,
        totalCategories: 0,
        totalClients: 0,
        totalPhoneNumbers: 0,
        activeReservations: 0,
        endedReservations: 0
    };

    $scope.searchText = "";
    $scope.stockFilter = "all";
    $scope.categoryFilter = null;

    $scope.currentPage = 1;
    $scope.pageSize = 20;
    $scope.totalCount = 0;
    $scope.totalPages = 0;

    $scope.sortColumn = "id";
    $scope.sortDirection = "asc";

    $scope.toast = {
        show: false,
        type: "success",
        title: "",
        message: ""
    };

    $scope.isLoading = false;
    $scope.loadingMessage = "Loading...";

    var toastTimer = null;

    function showToast(type, title, message) {
        if (toastTimer) {
            $timeout.cancel(toastTimer);
        }

        $scope.toast = {
            show: true,
            type: type,
            title: title,
            message: message
        };

        toastTimer = $timeout(function () {
            $scope.toast.show = false;
        }, 3000);
    }

    function startLoading(message) {
        $scope.loadingMessage = message || "Loading...";
        $scope.isLoading = true;
    }

    function stopLoading() {
        $scope.isLoading = false;
    }

    function showError(error, defaultMessage) {
        var message = defaultMessage;

        if (error && error.data) {
            if (typeof error.data === "string") {
                message = error.data;
            }
        }

        showToast("error", "Error", message);
    }


    $scope.loadCategories = function () {
        return $http.get("/api/categories/GetAllCategories")
            .then(function (response) {
                $scope.categories = response.data;
            }, function (error) {
                console.log(error);
                showError(error, "Error while loading categories");
            });
    };

    $scope.loadDevices = function () {
        startLoading("Loading devices...");

        $http.get("/api/devices/GetFilteredDevices", {
            params: {
                search: $scope.searchText,
                stockFilter: $scope.stockFilter,
                categoryId: $scope.categoryFilter,
                pageNumber: $scope.currentPage,
                pageSize: $scope.pageSize,
                sortColumn: $scope.sortColumn,
                sortDirection: $scope.sortDirection
            }
        }).then(function (response) {

            $scope.devices = response.data.items;
            $scope.totalCount = response.data.totalCount;
            $scope.currentPage = response.data.pageNumber;
            $scope.pageSize = response.data.pageSize;
            $scope.totalPages = response.data.totalPages;

            angular.forEach($scope.devices, function (device) {
                if (device.quantity === null || device.quantity === undefined) {
                    device.quantity = 0;
                }

                device.originalQuantity = device.quantity;
            });

        }, function (error) {
            console.log(error);
            showError(error, "Error while loading devices");
        }).finally(function () {
            stopLoading();
        });
    };

    $scope.searchDevices = function () {
        $scope.currentPage = 1;
        $scope.loadDevices();
    };


    $scope.toggleUserHomeVisibility = function (device) {
        $scope.isLoading = true;
        $scope.loadingMessage = "Updating device visibility...";

        $http.post("/api/devices/ToggleUserHomeVisibility/" + device.id)
            .then(function (response) {
                device.showOnUserHome = response.data.showOnUserHome;

                showToast(
                    "success",
                    "Updated",
                    response.data.message
                );
            })
            .catch(function (error) {
                var message = "Failed to update device visibility.";

                if (error.data) {
                    message = error.data;
                }

                showToast("error", "Error", message);
            })
            .finally(function () {
                $scope.isLoading = false;
            });
    };

    $scope.openDeviceModal = function (device) {

        var modalInstance = $uibModal.open({
            templateUrl: "deviceModal.html",
            controller: "DeviceModalController",
            resolve: {
                selectedDevice: function () {
                    if (device) {
                        return angular.copy(device);
                    }

                    return {
                        id: 0,
                        name: "",
                        quantity: 1,
                        categoryId: $scope.categories.length > 0 ? $scope.categories[0].id : null
                    };
                },
                categories: function () {
                    return $scope.categories;
                }
            }
        });

        modalInstance.result.then(function (savedDevice) {

            if (savedDevice.id === 0) {
                startLoading("Adding device...");

                $http.post("/api/devices/AddDevice", savedDevice)
                    .then(function () {
                        showToast("success", "Added", "Device added successfully.");
                        $scope.loadDevices();
                    }, function (error) {
                        showError(error, "Error while adding device");
                    }).finally(function () {
                        stopLoading();
                    });
            }
            else {
                startLoading("Updating device...");

                $http.put("/api/devices/UpdateDevice/" + savedDevice.id, savedDevice)
                    .then(function () {
                        showToast("success", "Updated", "Device updated successfully.");
                        $scope.loadDevices();
                    }, function (error) {
                        showError(error, "Error while editing device");
                    }).finally(function () {
                        stopLoading();
                    });
            }

        });
    };

    $scope.increaseQuantity = function (device) {
        var currentQuantity = parseInt(device.quantity || 0);
        device.quantity = currentQuantity + 1;
        $scope.updateQuantity(device);
    };

    $scope.decreaseQuantity = function (device) {
        var currentQuantity = parseInt(device.quantity || 0);

        if (currentQuantity <= 0) {
            return;
        }

        device.quantity = currentQuantity - 1;
        $scope.updateQuantity(device);
    };

    $scope.manualQuantityChange = function (device) {
        $scope.updateQuantity(device);
    };

    $scope.updateQuantity = function (device) {
        var quantity = parseInt(device.quantity);

        if (isNaN(quantity) || quantity < 0) {
            showToast("error", "Invalid Quantity", "Quantity must be a positive number or zero");
            device.quantity = device.originalQuantity || 0;
            return;
        }

        device.quantity = quantity;

        $http.put("/api/devices/UpdateDevice/" + device.id, device)
            .then(function (response) {
                device.quantity = response.data.quantity;
                device.originalQuantity = response.data.quantity;
            }, function (error) {
                device.quantity = device.originalQuantity || 0;
                showError(error, "Error while updating quantity");
            });
    };

    $scope.deleteDevice = function (device) {

        var modalInstance = $uibModal.open({
            templateUrl: "deleteConfirmModal.html",
            controller: "DeleteConfirmController",
            windowClass: "delete-modal-window",
            backdrop: "static",
            resolve: {
                selectedDevice: function () {
                    return device;
                }
            }
        });

        modalInstance.result.then(function (deviceToDelete) {

            startLoading("Deleting device...");

            $http.delete("/api/devices/DeleteDevice/" + deviceToDelete.id)
                .then(function () {
                    showToast("success", "Deleted", "Device deleted successfully.");
                    $scope.loadDevices();
                }, function (error) {
                    showError(error, "Error while deleting device");
                })
                .finally(function () {
                    stopLoading();
                });

        });
    };

    $scope.goToPage = function (page) {
        if (page < 1 || page > $scope.totalPages || page === $scope.currentPage) {
            return;
        }

        $scope.currentPage = page;
        $scope.loadDevices();
    };

    $scope.nextPage = function () {
        if ($scope.currentPage < $scope.totalPages) {
            $scope.currentPage++;
            $scope.loadDevices();
        }
    };

    $scope.previousPage = function () {
        if ($scope.currentPage > 1) {
            $scope.currentPage--;
            $scope.loadDevices();
        }
    };

    $scope.getPageNumbers = function () {
        var pages = [];
        var maxVisiblePages = 5;

        if ($scope.totalPages <= maxVisiblePages) {
            for (var i = 1; i <= $scope.totalPages; i++) {
                pages.push(i);
            }
        } else {
            var startPage = Math.max(1, $scope.currentPage - 2);
            var endPage = Math.min($scope.totalPages, startPage + maxVisiblePages - 1);

            if (endPage - startPage < maxVisiblePages - 1) {
                startPage = Math.max(1, endPage - maxVisiblePages + 1);
            }

            for (var j = startPage; j <= endPage; j++) {
                pages.push(j);
            }
        }

        return pages;
    };

    $scope.changeSort = function () {
        $scope.currentPage = 1;
        $scope.loadDevices();
    };

    $scope.toggleSortDirection = function () {
        $scope.sortDirection = $scope.sortDirection === "asc" ? "desc" : "asc";
        $scope.currentPage = 1;
        $scope.loadDevices();
    };


    $scope.loadCategories().then(function () {
        $scope.loadDevices();
    });
});

app.controller("UserHomeController", function ($scope, $http, $timeout) {

    $scope.devices = [];
    $scope.isLoading = false;
    $scope.loadingMessage = "Loading available devices...";

    $scope.toast = {
        show: false,
        type: "success",
        title: "",
        message: ""
    };

    function showToast(type, title, message) {
        $scope.toast = {
            show: true,
            type: type,
            title: title,
            message: message
        };

        $timeout(function () {
            $scope.toast.show = false;
        }, 2500);
    }

    $scope.loadUserHomeDevices = function () {
        $scope.isLoading = true;
        $scope.loadingMessage = "Loading available devices...";

        $http.get("/api/devices/GetUserHomeDevices")
            .then(function (response) {
                $scope.devices = response.data;
            })
            .catch(function () {
                showToast("error", "Error", "Failed to load available devices.");
            })
            .finally(function () {
                $scope.isLoading = false;
            });
    };

    $scope.addToCart = function (device) {
        $scope.isLoading = true;
        $scope.loadingMessage = "Adding device to cart...";

        $http.post("/api/cart/AddToCart/" + device.id)
            .then(function (response) {
                showToast("success", "Added", response.data.message);
            })
            .catch(function (error) {
                var message = "Failed to add device to cart.";

                if (error.data) {
                    message = error.data;
                }

                showToast("error", "Error", message);
            })
            .finally(function () {
                $scope.isLoading = false;
            });
    };

    $scope.loadUserHomeDevices();
});

app.controller("CartController", function ($scope, $http, $timeout) {

    $scope.cartItems = [];
    $scope.isLoading = false;
    $scope.loadingMessage = "Loading cart...";

    $scope.toast = {
        show: false,
        type: "success",
        title: "",
        message: ""
    };

    function showToast(type, title, message) {
        $scope.toast = {
            show: true,
            type: type,
            title: title,
            message: message
        };

        $timeout(function () {
            $scope.toast.show = false;
        }, 2500);
    }

    $scope.loadCart = function () {
        $scope.isLoading = true;
        $scope.loadingMessage = "Loading cart...";

        $http.get("/api/cart/GetMyCart")
            .then(function (response) {
                $scope.cartItems = response.data;
            })
            .catch(function () {
                showToast("error", "Error", "Failed to load cart.");
            })
            .finally(function () {
                $scope.isLoading = false;
            });
    };

    $scope.updateCartQuantity = function (item, newQuantity) {
        if (newQuantity < 1) {
            newQuantity = 1;
        }

        if (newQuantity > item.availableQuantity) {
            newQuantity = item.availableQuantity;
        }

        $scope.isLoading = true;
        $scope.loadingMessage = "Updating quantity...";

        $http.put("/api/cart/UpdateQuantity/" + item.id + "?quantity=" + newQuantity)
            .then(function (response) {
                item.quantity = newQuantity;
                showToast("success", "Updated", response.data.message);
            })
            .catch(function (error) {
                var message = "Failed to update quantity.";

                if (error.data) {
                    message = error.data;
                }

                showToast("error", "Error", message);
                $scope.loadCart();
            })
            .finally(function () {
                $scope.isLoading = false;
            });
    };

    $scope.increaseCartQuantity = function (item) {
        $scope.updateCartQuantity(item, item.quantity + 1);
    };

    $scope.decreaseCartQuantity = function (item) {
        $scope.updateCartQuantity(item, item.quantity - 1);
    };

    $scope.manualCartQuantityChange = function (item) {
        $scope.updateCartQuantity(item, item.quantity);
    };

    $scope.removeFromCart = function (item) {
        $scope.isLoading = true;
        $scope.loadingMessage = "Removing item...";

        $http.delete("/api/cart/RemoveFromCart/" + item.id)
            .then(function (response) {
                showToast("success", "Removed", response.data.message);
                $scope.loadCart();
            })
            .catch(function (error) {
                var message = "Failed to remove item.";

                if (error.data) {
                    message = error.data;
                }

                showToast("error", "Error", message);
            })
            .finally(function () {
                $scope.isLoading = false;
            });
    };

    $scope.clearCart = function () {
        $scope.isLoading = true;
        $scope.loadingMessage = "Clearing cart...";

        $http.delete("/api/cart/ClearCart")
            .then(function (response) {
                showToast("success", "Cleared", response.data.message);
                $scope.loadCart();
            })
            .catch(function () {
                showToast("error", "Error", "Failed to clear cart.");
            })
            .finally(function () {
                $scope.isLoading = false;
            });
    };

    $scope.submitRequest = function () {
        if (!$scope.cartItems || $scope.cartItems.length === 0) {
            showToast("error", "Empty Cart", "Please add devices before submitting a request.");
            return;
        }

        $scope.isLoading = true;
        $scope.loadingMessage = "Submitting request...";

        $http.post("/api/cart/SubmitRequest")
            .then(function (response) {
                showToast("success", "Submitted", response.data.message);
                $scope.loadCart();
            })
            .catch(function (error) {
                var message = "Failed to submit request.";

                if (error.data) {
                    message = error.data;
                }

                showToast("error", "Error", message);
            })
            .finally(function () {
                $scope.isLoading = false;
            });
    };

    $scope.loadCart();
});

app.controller("DeviceModalController", function ($scope, $uibModalInstance, selectedDevice, categories) {

    $scope.device = selectedDevice;
    $scope.categories = categories;
    $scope.errorMessage = "";

    if ($scope.device.quantity === null || $scope.device.quantity === undefined) {
        $scope.device.quantity = 1;
    }

    $scope.modalTitle = $scope.device.id === 0 ? "Add Device" : "Edit Device";

    function showModalError(message) {
        $scope.errorMessage = message;
    }

    $scope.save = function () {
        $scope.errorMessage = "";

        if (!$scope.device.name || $scope.device.name.trim() === "") {
            showModalError("Please enter device name");
            return;
        }

        if ($scope.device.name.trim().length > 50) {
            showModalError("Device name cannot exceed 50 characters");
            return;
        }

        var quantity = parseInt($scope.device.quantity);

        if (isNaN(quantity) || quantity < 0) {
            showModalError("Quantity must be a positive number or zero");
            return;
        }

        if (!$scope.device.categoryId || $scope.device.categoryId <= 0) {
            showModalError("Please select a category");
            return;
        }

        $scope.device.name = $scope.device.name.trim();
        $scope.device.quantity = quantity;

        $uibModalInstance.close($scope.device);
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss("cancel");
    };
});

app.controller("DeleteConfirmController", function ($scope, $uibModalInstance, selectedDevice) {

    $scope.deviceName = selectedDevice.name;

    $scope.confirmDelete = function () {
        $uibModalInstance.close(selectedDevice);
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss("cancel");
    };
});

/* ========================= Categories Page ========================= */

app.controller("CategoriesPageController", function ($scope, $http, $uibModal, $timeout) {

    $scope.categories = [];
    $scope.categorySearchText = "";

    $scope.toast = {
        show: false,
        type: "success",
        title: "",
        message: ""
    };

    $scope.isLoading = false;
    $scope.loadingMessage = "Loading...";

    var toastTimer = null;

    function showToast(type, title, message) {
        if (toastTimer) {
            $timeout.cancel(toastTimer);
        }

        $scope.toast = {
            show: true,
            type: type,
            title: title,
            message: message
        };

        toastTimer = $timeout(function () {
            $scope.toast.show = false;
        }, 3000);
    }

    function startLoading(message) {
        $scope.loadingMessage = message || "Loading...";
        $scope.isLoading = true;
    }

    function stopLoading() {
        $scope.isLoading = false;
    }

    function showError(error, defaultMessage) {
        var message = defaultMessage;

        if (error && error.data) {
            if (typeof error.data === "string") {
                message = error.data;
            }
        }

        showToast("error", "Error", message);
    }

    $scope.loadCategories = function () {
        startLoading("Loading categories...");

        $http.get("/api/categories/GetAllCategories")
            .then(function (response) {
                $scope.categories = response.data;
            }, function (error) {
                console.log(error);
                showError(error, "Error while loading categories");
            })
            .finally(function () {
                stopLoading();
            });
    };

    $scope.openCategoryModal = function (category) {

        var modalInstance = $uibModal.open({
            templateUrl: "categoryModal.html",
            controller: "CategoryModalController",
            resolve: {
                selectedCategory: function () {
                    if (category) {
                        return angular.copy(category);
                    }

                    return {
                        id: 0,
                        name: ""
                    };
                }
            }
        });

        modalInstance.result.then(function (savedCategory) {

            if (savedCategory.id === 0) {
                startLoading("Adding category...");

                $http.post("/api/categories/AddCategory", savedCategory)
                    .then(function () {
                        showToast("success", "Added", "Category added successfully.");
                        $scope.loadCategories();
                    }, function (error) {
                        showError(error, "Error while adding category");
                    })
                    .finally(function () {
                        stopLoading();
                    });
            }
            else {
                startLoading("Updating category...");

                $http.put("/api/categories/UpdateCategory/" + savedCategory.id, savedCategory)
                    .then(function () {
                        showToast("success", "Updated", "Category updated successfully.");
                        $scope.loadCategories();
                    }, function (error) {
                        showError(error, "Error while editing category");
                    })
                    .finally(function () {
                        stopLoading();
                    });
            }

        });
    };

    $scope.deleteCategory = function (category) {

        var modalInstance = $uibModal.open({
            templateUrl: "deleteCategoryModal.html",
            controller: "DeleteCategoryController",
            windowClass: "delete-modal-window",
            backdrop: "static",
            resolve: {
                selectedCategory: function () {
                    return category;
                }
            }
        });

        modalInstance.result.then(function (categoryToDelete) {

            startLoading("Deleting category...");

            $http.delete("/api/categories/DeleteCategory/" + categoryToDelete.id)
                .then(function () {
                    showToast("success", "Deleted", "Category deleted successfully.");
                    $scope.loadCategories();
                }, function (error) {
                    showError(error, "Error while deleting category");
                })
                .finally(function () {
                    stopLoading();
                });

        });
    };

    $scope.loadCategories();
});


app.controller("RequestsController", function ($scope, $http, $timeout) {

    $scope.requests = [];
    $scope.statusFilter = "";
    $scope.isLoading = false;
    $scope.loadingMessage = "Loading requests...";

    $scope.toast = {
        show: false,
        type: "success",
        title: "",
        message: ""
    };

    function showToast(type, title, message) {
        $scope.toast = {
            show: true,
            type: type,
            title: title,
            message: message
        };

        $timeout(function () {
            $scope.toast.show = false;
        }, 2500);
    }

    $scope.loadRequests = function () {
        $scope.isLoading = true;
        $scope.loadingMessage = "Loading requests...";

        $http.get("/api/requests/GetAllRequests", {
            params: {
                status: $scope.statusFilter
            }
        })
            .then(function (response) {
                $scope.requests = response.data;
            })
            .catch(function () {
                showToast("error", "Error", "Failed to load requests.");
            })
            .finally(function () {
                $scope.isLoading = false;
            });
    };

    $scope.acceptRequest = function (request) {
        $scope.isLoading = true;
        $scope.loadingMessage = "Accepting request...";

        $http.post("/api/requests/AcceptRequest/" + request.id)
            .then(function (response) {
                showToast("success", "Accepted", response.data.message);
                $scope.loadRequests();
            })
            .catch(function (error) {
                var message = "Failed to accept request.";

                if (error.data) {
                    message = error.data;
                }

                showToast("error", "Error", message);
            })
            .finally(function () {
                $scope.isLoading = false;
            });
    };

    $scope.rejectRequest = function (request) {
        $scope.isLoading = true;
        $scope.loadingMessage = "Rejecting request...";

        $http.post("/api/requests/RejectRequest/" + request.id)
            .then(function (response) {
                showToast("success", "Rejected", response.data.message);
                $scope.loadRequests();
            })
            .catch(function (error) {
                var message = "Failed to reject request.";

                if (error.data) {
                    message = error.data;
                }

                showToast("error", "Error", message);
            })
            .finally(function () {
                $scope.isLoading = false;
            });
    };

    $scope.loadRequests();
});

app.controller("UsersController", function ($scope, $http, $timeout, $uibModal) {

    $scope.users = [];
    $scope.isLoading = false;
    $scope.loadingMessage = "Loading users...";

    $scope.toast = {
        show: false,
        type: "success",
        title: "",
        message: ""
    };

    function showToast(type, title, message) {
        $scope.toast = {
            show: true,
            type: type,
            title: title,
            message: message
        };

        $timeout(function () {
            $scope.toast.show = false;
        }, 2500);
    }

    $scope.loadUsers = function () {
        $scope.isLoading = true;
        $scope.loadingMessage = "Loading users...";

        $http.get("/api/users/GetAllUsers")
            .then(function (response) {
                $scope.users = response.data;
            })
            .catch(function () {
                showToast("error", "Error", "Failed to load users.");
            })
            .finally(function () {
                $scope.isLoading = false;
            });
    };

    $scope.openCreateUserModal = function () {
        var modalInstance = $uibModal.open({
            templateUrl: "createUserModal.html",
            controller: "CreateUserModalController",
            backdrop: "static",
            keyboard: false
        });

        modalInstance.result.then(function () {
            showToast("success", "Created", "User created successfully.");
            $scope.loadUsers();
        });
    };

    $scope.loadUsers();
});

app.controller("CreateUserModalController", function ($scope, $http, $uibModalInstance) {

    $scope.user = {
        fullName: "",
        phoneNumber: "",
        email: "",
        password: ""
    };

    $scope.errorMessage = "";

    $scope.save = function () {
        $scope.errorMessage = "";

        if (!$scope.user.fullName || !$scope.user.fullName.trim()) {
            $scope.errorMessage = "Full name is required.";
            return;
        }

        if (!$scope.user.phoneNumber || !$scope.user.phoneNumber.trim()) {
            $scope.errorMessage = "Mobile number is required.";
            return;
        }

        if (!$scope.user.email || !$scope.user.email.trim()) {
            $scope.errorMessage = "Email is required.";
            return;
        }

        if (!$scope.user.password || $scope.user.password.length < 6) {
            $scope.errorMessage = "Password must be at least 6 characters.";
            return;
        }

        $http.post("/api/users/CreateUser", $scope.user)
            .then(function () {
                $scope.user = {
                    fullName: "",
                    phoneNumber: "",
                    email: "",
                    password: ""
                };

                $uibModalInstance.close();
            })
            .catch(function (error) {
                if (error.data) {
                    $scope.errorMessage = error.data;
                } else {
                    $scope.errorMessage = "Failed to create user.";
                }
            });
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss("cancel");
    };
});


app.controller("CategoryModalController", function ($scope, $uibModalInstance, selectedCategory) {

    $scope.category = selectedCategory;
    $scope.errorMessage = "";
    $scope.modalTitle = $scope.category.id === 0 ? "Add Category" : "Edit Category";

    function showModalError(message) {
        $scope.errorMessage = message;
    }

    $scope.save = function () {
        $scope.errorMessage = "";

        if (!$scope.category.name || $scope.category.name.trim() === "") {
            showModalError("Please enter category name");
            return;
        }

        if ($scope.category.name.trim().length > 50) {
            showModalError("Category name cannot exceed 50 characters");
            return;
        }

        $scope.category.name = $scope.category.name.trim();

        $uibModalInstance.close($scope.category);
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss("cancel");
    };
});

app.controller("DeleteCategoryController", function ($scope, $uibModalInstance, selectedCategory) {

    $scope.categoryName = selectedCategory.name;

    $scope.confirmDelete = function () {
        $uibModalInstance.close(selectedCategory);
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss("cancel");
    };
});

/* ========================= Clients Page ========================= */

app.controller("ClientsPageController", function ($scope, $http, $uibModal, $timeout) {

    $scope.clients = [];
    $scope.clientSearchText = "";
    $scope.clientTypeFilter = "";

    $scope.toast = {
        show: false,
        type: "success",
        title: "",
        message: ""
    };

    $scope.isLoading = false;
    $scope.loadingMessage = "Loading...";

    var toastTimer = null;

    function showToast(type, title, message) {
        if (toastTimer) {
            $timeout.cancel(toastTimer);
        }

        $scope.toast = {
            show: true,
            type: type,
            title: title,
            message: message
        };

        toastTimer = $timeout(function () {
            $scope.toast.show = false;
        }, 3000);
    }

    function startLoading(message) {
        $scope.loadingMessage = message || "Loading...";
        $scope.isLoading = true;
    }

    function stopLoading() {
        $scope.isLoading = false;
    }

    function showError(error, defaultMessage) {
        var message = defaultMessage;

        if (error && error.data) {
            if (typeof error.data === "string") {
                message = error.data;
            }
        }

        showToast("error", "Error", message);
    }


    $scope.loadClients = function () {
        startLoading("Loading clients...");

        $http.get("/api/clients/GetAllClients")
            .then(function (response) {
                $scope.clients = response.data;
            }, function (error) {
                console.log(error);
                showError(error, "Error while loading clients");
            })
            .finally(function () {
                stopLoading();
            });
    };

    $scope.filterClientType = function (client) {
        if (!$scope.clientTypeFilter) {
            return true;
        }

        return client.type === $scope.clientTypeFilter;
    };

    $scope.openClientModal = function (client) {

        var modalInstance = $uibModal.open({
            templateUrl: "clientModal.html",
            controller: "ClientModalController",
            resolve: {
                selectedClient: function () {
                    if (client) {
                        var copiedClient = angular.copy(client);

                        if (copiedClient.birthDate) {
                            copiedClient.birthDate = new Date(copiedClient.birthDate);
                        }

                        return copiedClient;
                    }

                    return {
                        id: 0,
                        name: "",
                        type: 1,
                        birthDate: null
                    };
                }
            }
        });

        modalInstance.result.then(function (savedClient) {

            if (savedClient.id === 0) {
                startLoading("Adding client...");

                $http.post("/api/clients/AddClient", savedClient)
                    .then(function () {
                        showToast("success", "Added", "Client added successfully.");
                        $scope.loadClients();
                    }, function (error) {
                        showError(error, "Error while adding client");
                    })
                    .finally(function () {
                        stopLoading();
                    });
            }
            else {
                startLoading("Updating client...");

                $http.put("/api/clients/UpdateClient/" + savedClient.id, savedClient)
                    .then(function () {
                        showToast("success", "Updated", "Client updated successfully.");
                        $scope.loadClients();
                    }, function (error) {
                        showError(error, "Error while editing client");
                    })
                    .finally(function () {
                        stopLoading();
                    });
            }

        });
    };



    $scope.deleteClient = function (client) {

        var modalInstance = $uibModal.open({
            templateUrl: "deleteClientModal.html",
            controller: "DeleteClientController",
            windowClass: "delete-modal-window",
            backdrop: "static",
            resolve: {
                selectedClient: function () {
                    return client;
                }
            }
        });

        modalInstance.result.then(function (clientToDelete) {

            startLoading("Deleting client...");

            $http.delete("/api/clients/DeleteClient/" + clientToDelete.id)
                .then(function () {
                    showToast("success", "Deleted", "Client deleted successfully.");
                    $scope.loadClients();
                }, function (error) {
                    showError(error, "Error while deleting client");
                })
                .finally(function () {
                    stopLoading();
                });

        });
    };

    $scope.openReserveModal = function (client) {

        var modalInstance = $uibModal.open({
            templateUrl: "reservePhoneNumberModal.html",
            controller: "ReservePhoneNumberModalController",
            resolve: {
                selectedClient: function () {
                    return client;
                }
            }
        });

        modalInstance.result.then(function (reservationRequest) {

            startLoading("Reserving phone number...");

            $http.post("/api/phoneNumberReservations/ReservePhoneNumber", reservationRequest)
                .then(function () {
                    showToast("success", "Reserved", "Phone number reserved successfully.");
                    $scope.loadClients();
                }, function (error) {
                    showError(error, "Error while reserving phone number");
                })
                .finally(function () {
                    stopLoading();
                });

        });
    };

    $scope.openUnreserveModal = function (client) {

        var modalInstance = $uibModal.open({
            templateUrl: "unreservePhoneNumberModal.html",
            controller: "UnreservePhoneNumberModalController",
            resolve: {
                selectedClient: function () {
                    return client;
                }
            }
        });

        modalInstance.result.then(function (unreservationRequest) {

            startLoading("Unreserving phone number...");

            $http.post("/api/phoneNumberReservations/UnreservePhoneNumber", unreservationRequest)
                .then(function () {
                    showToast("success", "Unreserved", "Phone number unreserved successfully.");
                    $scope.loadClients();
                }, function (error) {
                    showError(error, "Error while unreserving phone number");
                })
                .finally(function () {
                    stopLoading();
                });

        });
    };

    $scope.loadClients();
});

app.controller("ClientModalController", function ($scope, $uibModalInstance, selectedClient) {

    $scope.client = selectedClient;
    $scope.errorMessage = "";
    $scope.modalTitle = $scope.client.id === 0 ? "Add Client" : "Edit Client";

    function showModalError(message) {
        $scope.errorMessage = message;
    }

    $scope.onTypeChange = function (selectedValue) {
        if (selectedValue) {
            $scope.client.type = selectedValue;
        }

        if ($scope.client.type === 2) {
            $scope.client.birthDate = null;
        }
    };

    $scope.save = function () {
        $scope.errorMessage = "";

        if (!$scope.client.name || $scope.client.name.trim() === "") {
            showModalError("Please enter client name");
            return;
        }

        if ($scope.client.name.trim().length > 50) {
            showModalError("Client name cannot exceed 50 characters");
            return;
        }

        if (!$scope.client.type) {
            showModalError("Please select client type");
            return;
        }

        if ($scope.client.type !== 1 && $scope.client.type !== 2) {
            showModalError("Invalid client type");
            return;
        }

        if ($scope.client.type === 1) {
            if (!$scope.client.birthDate) {
                showModalError("Birth date is required for individual clients");
                return;
            }

            var today = new Date();
            today.setHours(0, 0, 0, 0);

            var birthDate = new Date($scope.client.birthDate);
            birthDate.setHours(0, 0, 0, 0);

            if (birthDate > today) {
                showModalError("Birth date cannot be in the future");
                return;
            }
        }

        if ($scope.client.type === 2) {
            $scope.client.birthDate = null;
        }

        $scope.client.name = $scope.client.name.trim();

        $uibModalInstance.close($scope.client);
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss("cancel");
    };
});

app.controller("DeleteClientController", function ($scope, $uibModalInstance, selectedClient) {

    $scope.clientName = selectedClient.name;

    $scope.confirmDelete = function () {
        $uibModalInstance.close(selectedClient);
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss("cancel");
    };
});

app.controller("ReservePhoneNumberModalController", function ($scope, $http, $uibModalInstance, selectedClient) {

    $scope.clientId = selectedClient.id;
    $scope.clientName = selectedClient.name;

    $scope.availablePhoneNumbers = [];
    $scope.selectedPhoneNumberId = null;
    $scope.errorMessage = "";

    function showModalError(message) {
        $scope.errorMessage = message;
    }

    $scope.loadAvailablePhoneNumbers = function () {
        $http.get("/api/phoneNumberReservations/GetAvailablePhoneNumbers")
            .then(function (response) {
                $scope.availablePhoneNumbers = response.data;

                if ($scope.availablePhoneNumbers.length > 0) {
                    $scope.selectedPhoneNumberId = $scope.availablePhoneNumbers[0].id;
                }
            }, function (error) {
                console.log(error);
                showModalError("Error while loading available phone numbers");
            });
    };

    $scope.save = function () {
        $scope.errorMessage = "";

        if (!$scope.selectedPhoneNumberId || $scope.selectedPhoneNumberId <= 0) {
            showModalError("Please select an available phone number");
            return;
        }

        $uibModalInstance.close({
            clientId: $scope.clientId,
            phoneNumberId: $scope.selectedPhoneNumberId
        });
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss("cancel");
    };

    $scope.loadAvailablePhoneNumbers();
});

app.controller("UnreservePhoneNumberModalController", function ($scope, $http, $uibModalInstance, selectedClient) {

    $scope.clientId = selectedClient.id;
    $scope.clientName = selectedClient.name;

    $scope.reservedPhoneNumbers = [];
    $scope.selectedPhoneNumberId = null;
    $scope.errorMessage = "";

    function showModalError(message) {
        $scope.errorMessage = message;
    }

    $scope.loadReservedPhoneNumbers = function () {
        $http.get("/api/phoneNumberReservations/GetClientActivePhoneNumbers/" + $scope.clientId)
            .then(function (response) {
                $scope.reservedPhoneNumbers = response.data;

                if ($scope.reservedPhoneNumbers.length > 0) {
                    $scope.selectedPhoneNumberId = $scope.reservedPhoneNumbers[0].id;
                }
            }, function (error) {
                console.log(error);
                showModalError("Error while loading reserved phone numbers");
            });
    };

    $scope.save = function () {
        $scope.errorMessage = "";

        if (!$scope.selectedPhoneNumberId || $scope.selectedPhoneNumberId <= 0) {
            showModalError("Please select a reserved phone number");
            return;
        }

        $uibModalInstance.close({
            clientId: $scope.clientId,
            phoneNumberId: $scope.selectedPhoneNumberId
        });
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss("cancel");
    };

    $scope.loadReservedPhoneNumbers();
});

/* ========================= Phone Numbers Page ========================= */

app.controller("PhoneNumbersPageController", function ($scope, $http, $uibModal, $timeout) {

    $scope.phoneNumbers = [];
    $scope.devices = [];

    $scope.phoneSearchText = "";
    $scope.deviceFilter = null;

    $scope.toast = {
        show: false,
        type: "success",
        title: "",
        message: ""
    };

    $scope.isLoading = false;
    $scope.loadingMessage = "Loading...";

    var toastTimer = null;

    function showToast(type, title, message) {
        if (toastTimer) {
            $timeout.cancel(toastTimer);
        }

        $scope.toast = {
            show: true,
            type: type,
            title: title,
            message: message
        };

        toastTimer = $timeout(function () {
            $scope.toast.show = false;
        }, 3000);
    }

    function startLoading(message) {
        $scope.loadingMessage = message || "Loading...";
        $scope.isLoading = true;
    }

    function stopLoading() {
        $scope.isLoading = false;
    }

    function showError(error, defaultMessage) {
        var message = defaultMessage;

        if (error && error.data) {
            if (typeof error.data === "string") {
                message = error.data;
            }
        }

        showToast("error", "Error", message);
    }

    $scope.loadDevices = function () {
        return $http.get("/api/devices/GetAllDevices")
            .then(function (response) {
                $scope.devices = response.data;
            }, function (error) {
                console.log(error);
                showError(error, "Error while loading devices");
            });
    };

    $scope.loadPhoneNumbers = function () {
        startLoading("Loading phone numbers...");

        $http.get("/api/phoneNumbers/GetAllPhoneNumbers")
            .then(function (response) {
                $scope.phoneNumbers = response.data;
            }, function (error) {
                console.log(error);
                showError(error, "Error while loading phone numbers");
            })
            .finally(function () {
                stopLoading();
            });
    };

    $scope.filterByDevice = function (phoneNumber) {
        if (!$scope.deviceFilter) {
            return true;
        }

        return phoneNumber.deviceId === $scope.deviceFilter;
    };

    $scope.openPhoneNumberModal = function (phoneNumber) {

        var modalInstance = $uibModal.open({
            templateUrl: "phoneNumberModal.html",
            controller: "PhoneNumberModalController",
            resolve: {
                selectedPhoneNumber: function () {
                    if (phoneNumber) {
                        return angular.copy(phoneNumber);
                    }

                    return {
                        id: 0,
                        number: "",
                        deviceId: $scope.devices.length > 0 ? $scope.devices[0].id : null
                    };
                },
                devices: function () {
                    return $scope.devices;
                }
            }
        });

        modalInstance.result.then(function (savedPhoneNumber) {

            if (savedPhoneNumber.id === 0) {
                startLoading("Adding phone number...");

                $http.post("/api/phoneNumbers/AddPhoneNumber", savedPhoneNumber)
                    .then(function () {
                        showToast("success", "Added", "Phone number added successfully.");
                        $scope.loadPhoneNumbers();
                    }, function (error) {
                        showError(error, "Error while adding phone number");
                    })
                    .finally(function () {
                        stopLoading();
                    });
            }
            else {
                startLoading("Updating phone number...");

                $http.put("/api/phoneNumbers/UpdatePhoneNumber/" + savedPhoneNumber.id, savedPhoneNumber)
                    .then(function () {
                        showToast("success", "Updated", "Phone number updated successfully.");
                        $scope.loadPhoneNumbers();
                    }, function (error) {
                        showError(error, "Error while editing phone number");
                    })
                    .finally(function () {
                        stopLoading();
                    });
            }

        });
    };

    $scope.deletePhoneNumber = function (phoneNumber) {

        var modalInstance = $uibModal.open({
            templateUrl: "deletePhoneNumberModal.html",
            controller: "DeletePhoneNumberController",
            windowClass: "delete-modal-window",
            backdrop: "static",
            resolve: {
                selectedPhoneNumber: function () {
                    return phoneNumber;
                }
            }
        });

        modalInstance.result.then(function (phoneNumberToDelete) {

            startLoading("Deleting phone number...");

            $http.delete("/api/phoneNumbers/DeletePhoneNumber/" + phoneNumberToDelete.id)
                .then(function () {
                    showToast("success", "Deleted", "Phone number deleted successfully.");
                    $scope.loadPhoneNumbers();
                }, function (error) {
                    showError(error, "Error while deleting phone number");
                })
                .finally(function () {
                    stopLoading();
                });

        });
    };

    $scope.loadDevices().then(function () {
        $scope.loadPhoneNumbers();
    });
});

app.controller("PhoneNumberModalController", function ($scope, $uibModalInstance, selectedPhoneNumber, devices) {

    $scope.phoneNumber = selectedPhoneNumber;
    $scope.devices = devices;
    $scope.errorMessage = "";
    $scope.modalTitle = $scope.phoneNumber.id === 0 ? "Add Phone Number" : "Edit Phone Number";

    function showModalError(message) {
        $scope.errorMessage = message;
    }

    function isValidPhoneNumber(number) {
        for (var i = 0; i < number.length; i++) {
            var character = number.charAt(i);

            var isAllowed =
                /[0-9]/.test(character) ||
                character === "+" ||
                character === "-" ||
                character === "(" ||
                character === ")" ||
                character === " ";

            if (!isAllowed) {
                return false;
            }
        }

        return true;
    }

    $scope.save = function () {
        $scope.errorMessage = "";

        if (!$scope.phoneNumber.number || $scope.phoneNumber.number.trim() === "") {
            showModalError("Please enter phone number");
            return;
        }

        if ($scope.phoneNumber.number.trim().length > 20) {
            showModalError("Phone number cannot exceed 20 characters");
            return;
        }

        if (!isValidPhoneNumber($scope.phoneNumber.number.trim())) {
            showModalError("Phone number can contain only digits, spaces, +, -, and parentheses");
            return;
        }

        if (!$scope.phoneNumber.deviceId || $scope.phoneNumber.deviceId <= 0) {
            showModalError("Please select a device");
            return;
        }

        $scope.phoneNumber.number = $scope.phoneNumber.number.trim();

        $uibModalInstance.close($scope.phoneNumber);
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss("cancel");
    };
});

app.controller("DeletePhoneNumberController", function ($scope, $uibModalInstance, selectedPhoneNumber) {

    $scope.phoneNumberValue = selectedPhoneNumber.number;

    $scope.confirmDelete = function () {
        $uibModalInstance.close(selectedPhoneNumber);
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss("cancel");
    };
});

/* ========================= Reservations Report Page ========================= */

app.controller("PhoneNumberReservationsPageController", function ($scope, $http, $uibModal, $timeout) {

    $scope.reservations = [];
    $scope.clients = [];
    $scope.phoneNumbers = [];

    $scope.clientFilter = null;
    $scope.phoneNumberFilter = null;
    $scope.statusFilter = "";

    $scope.toast = {
        show: false,
        type: "success",
        title: "",
        message: ""
    };

    $scope.isLoading = false;
    $scope.loadingMessage = "Loading...";

    var toastTimer = null;

    function showToast(type, title, message) {
        if (toastTimer) {
            $timeout.cancel(toastTimer);
        }

        $scope.toast = {
            show: true,
            type: type,
            title: title,
            message: message
        };

        toastTimer = $timeout(function () {
            $scope.toast.show = false;
        }, 3000);
    }

    function startLoading(message) {
        $scope.loadingMessage = message || "Loading...";
        $scope.isLoading = true;
    }

    function stopLoading() {
        $scope.isLoading = false;
    }

    function showError(error, defaultMessage) {
        var message = defaultMessage;

        if (error && error.data) {
            if (typeof error.data === "string") {
                message = error.data;
            }
        }

        showToast("error", "Error", message);
    }

    $scope.loadClients = function () {
        return $http.get("/api/clients/GetAllClients")
            .then(function (response) {
                $scope.clients = response.data;
            }, function (error) {
                console.log(error);
                showError(error, "Error while loading clients");
            });
    };

    $scope.loadPhoneNumbers = function () {
        return $http.get("/api/phoneNumbers/GetAllPhoneNumbers")
            .then(function (response) {
                $scope.phoneNumbers = response.data;
            }, function (error) {
                console.log(error);
                showError(error, "Error while loading phone numbers");
            });
    };

    $scope.loadReservations = function () {
        startLoading("Loading reservations...");

        $http.get("/api/phoneNumberReservations/GetFilteredReservations", {
            params: {
                clientId: $scope.clientFilter || null,
                phoneNumberId: $scope.phoneNumberFilter || null,
                status: $scope.statusFilter || null
            }
        }).then(function (response) {
            $scope.reservations = response.data;
        }, function (error) {
            console.log(error);
            showError(error, "Error while loading reservations");
        }).finally(function () {
            stopLoading();
        });
    };

    $scope.exportReservationsWord = function () {
        var url = "/api/phoneNumberReservations/ExportReservationsWord";

        var queryParams = [];

        if ($scope.clientFilter) {
            queryParams.push("clientId=" + $scope.clientFilter);
        }

        if ($scope.phoneNumberFilter) {
            queryParams.push("phoneNumberId=" + $scope.phoneNumberFilter);
        }

        if ($scope.statusFilter) {
            queryParams.push("status=" + $scope.statusFilter);
        }

        if (queryParams.length > 0) {
            url += "?" + queryParams.join("&");
        }

        window.location.href = url;
    };

    $scope.openReportModal = function () {
        var modalInstance = $uibModal.open({
            templateUrl: "generateReportModal.html",
            controller: "GenerateReportModalController"
        });

        modalInstance.result.then(function (format) {
            if (format === "word") {
                $scope.exportReservationsWord();
            }

            if (format === "pdf") {
                $scope.exportReservationsPdf();
            }
        });
    };

    $scope.exportReservationsPdf = function () {
        setTimeout(function () {
            window.print();
        }, 300);
    };

    $scope.loadClients()
        .then(function () {
            return $scope.loadPhoneNumbers();
        })
        .then(function () {
            $scope.loadReservations();
        });
});

app.controller("GenerateReportModalController", function ($scope, $uibModalInstance, $timeout) {

    $scope.chooseFormat = function (format) {
        $uibModalInstance.close(format);
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss("cancel");
    };
});

/* ========================= Reports Page ========================= */

app.controller("ReportsPageController", function ($scope, $http, $uibModal, $timeout) {

    $scope.activeReportTab = "overview";

    $scope.centralImport = {
        importType: "clients",
        file: null
    };

    $scope.clientsReport = [];

    $scope.clientsReportFilters = {
        clientType: null,
        reservationActivity: ""
    };

    $scope.devicesReport = [];
    $scope.categories = [];
    $scope.devices = [];

    $scope.reservationsReport = [];

    $scope.reservationsReportFilters = {
        clientId: null,
        phoneNumberId: null,
        status: ""
    };

    $scope.reportClients = [];
    $scope.reportPhoneNumbers = [];

    $scope.overview = {
        availablePhoneNumbers: 0,
        reservedPhoneNumbers: 0,
        clientsWithoutActiveReservations: 0,
        clientsWithActiveReservations: 0,
        mostActiveClientName: "",
        mostActiveClientReservations: 0,
        mostReservedDeviceName: "",
        mostReservedDeviceCount: 0
    };



    $scope.devicesReportFilters = {
        categoryId: null,
        deviceId: null,
        phoneStatus: ""
    };

    $scope.toast = {
        show: false,
        type: "success",
        title: "",
        message: ""
    };

    $scope.isLoading = false;
    $scope.loadingMessage = "Loading...";

    var toastTimer = null;

    function showToast(type, title, message) {
        if (toastTimer) {
            $timeout.cancel(toastTimer);
        }

        $scope.toast = {
            show: true,
            type: type,
            title: title,
            message: message
        };

        toastTimer = $timeout(function () {
            $scope.toast.show = false;
        }, 3000);
    }

    function startLoading(message) {
        $scope.loadingMessage = message || "Loading...";
        $scope.isLoading = true;
    }

    function stopLoading() {
        $scope.isLoading = false;
    }

    function showError(error, defaultMessage) {
        var message = defaultMessage;

        if (error && error.data) {
            if (typeof error.data === "string") {
                message = error.data;
            }
        }

        showToast("error", "Error", message);
    }

    $scope.importCentralData = function () {

        if (!$scope.centralImport.importType) {
            showToast("error", "Import Type Required", "Please select an import type.");
            return;
        }

        if (!$scope.centralImport.file) {
            showToast("error", "File Required", "Please select a CSV file.");
            return;
        }

        var fileName = $scope.centralImport.file.name.toLowerCase();

        if (!fileName.endsWith(".csv")) {
            showToast("error", "Invalid File", "Only CSV files are allowed.");
            return;
        }

        var url = "";

        if ($scope.centralImport.importType === "clients") {
            url = "/api/clients/ImportClients";
        }
        else if ($scope.centralImport.importType === "categories") {
            url = "/api/categories/ImportCategories";
        }
        else if ($scope.centralImport.importType === "devices") {
            url = "/api/devices/ImportDevices";
        }
        else if ($scope.centralImport.importType === "phoneNumbers") {
            url = "/api/phoneNumbers/ImportPhoneNumbers";
        }
        else {
            showToast("error", "Coming Soon", "This import type is not available yet.");
            return;
        }

        startLoading("Importing data...");

        var formData = new FormData();
        formData.append("file", $scope.centralImport.file);

        $http.post(url, formData, {
            transformRequest: angular.identity,
            headers: {
                "Content-Type": undefined
            }
        }).then(function (response) {

            var result = response.data;

            showToast(
                "success",
                "Import Finished",
                result.importedRows + " imported, " + result.skippedRows + " skipped."
            );

            $scope.openCentralImportResultModal(result);

            $scope.loadSummary();
            $scope.loadReportsOverview();

            $scope.centralImport.file = null;

        }, function (error) {
            showError(error, "Error while importing data");
        }).finally(function () {
            stopLoading();
        });
    };

    $scope.openCentralImportResultModal = function (result) {
        $uibModal.open({
            templateUrl: "centralImportResultModal.html",
            controller: "CentralImportResultModalController",
            size: "lg",
            windowClass: "import-result-modal-window",
            backdrop: "static",
            resolve: {
                importResult: function () {
                    return result;
                }
            }
        });
    };

    $scope.openCentralImportHistoryModal = function () {
        $uibModal.open({
            templateUrl: "centralImportHistoryModal.html",
            controller: "CentralImportHistoryModalController",
            size: "lg",
            windowClass: "import-history-modal-window",
            backdrop: "static"
        });
    };

    $scope.loadClientsReport = function () {
        startLoading("Loading clients report...");

        $http.get("/api/reports/GetClientsReport", {
            params: {
                clientType: $scope.clientsReportFilters.clientType || null,
                reservationActivity: $scope.clientsReportFilters.reservationActivity || null
            }
        }).then(function (response) {
            $scope.clientsReport = response.data;
        }, function (error) {
            console.log(error);
            showError(error, "Error while loading clients report");
        }).finally(function () {
            stopLoading();
        });
    };

    $scope.exportClientsReportWord = function () {
        var url = "/api/reports/ExportClientsReportWord";

        var queryParams = [];

        if ($scope.clientsReportFilters.clientType) {
            queryParams.push("clientType=" + $scope.clientsReportFilters.clientType);
        }

        if ($scope.clientsReportFilters.reservationActivity) {
            queryParams.push("reservationActivity=" + $scope.clientsReportFilters.reservationActivity);
        }

        if (queryParams.length > 0) {
            url += "?" + queryParams.join("&");
        }

        window.location.href = url;
    };

    $scope.exportClientsReportPdf = function () {
        $scope.printMode = "clients";

        setTimeout(function () {
            window.print();
            $scope.printMode = "";
            $scope.$applyAsync();
        }, 300);
    };

    $scope.openClientsReportModal = function () {
        var modalInstance = $uibModal.open({
            templateUrl: "generateClientsReportModal.html",
            controller: "GenerateReportModalController"
        });

        modalInstance.result.then(function (format) {
            if (format === "word") {
                $scope.exportClientsReportWord();
            }

            if (format === "pdf") {
                $scope.exportClientsReportPdf();
            }
        });
    };

    $scope.setReportTab = function (tabName) {
        $scope.activeReportTab = tabName;

        if (tabName === "overview") {
            $scope.loadSummary();
            $scope.loadReportsOverview();
        }

        if (tabName === "clients") {
            $scope.loadClientsReport();
        }

        if (tabName === "devices") {
            $scope.loadReportCategories();
            $scope.loadReportDevices();
            $scope.loadDevicesReport();
        }

        if (tabName === "reservations") {
            $scope.loadReportClients();
            $scope.loadReportPhoneNumbers();
            $scope.loadReservationsReport();
        }

        if (tabName === "imports") {
            $scope.centralImport.importType = "clients";
        }
    };

    $scope.loadSummary = function () {
        startLoading("Loading reports summary...");

        $http.get("/api/dashboard/GetSummary")
            .then(function (response) {
                $scope.summary = response.data;
            }, function (error) {
                console.log(error);
                showError(error, "Error while loading reports summary");
            })
            .finally(function () {
                stopLoading();
            });
    };

    $scope.loadReportsOverview = function () {
        return $http.get("/api/reports/GetReportsOverview")
            .then(function (response) {
                $scope.overview = response.data;
            }, function (error) {
                console.log(error);
                showError(error, "Error while loading reports overview");
            });
    };

    $scope.loadReportCategories = function () {
        return $http.get("/api/categories/GetAllCategories")
            .then(function (response) {
                $scope.categories = response.data;
            }, function (error) {
                console.log(error);
                showError(error, "Error while loading categories");
            });
    };

    $scope.loadReportDevices = function () {
        return $http.get("/api/devices/GetAllDevices")
            .then(function (response) {
                $scope.devices = response.data;
            }, function (error) {
                console.log(error);
                showError(error, "Error while loading devices");
            });
    };

    $scope.loadDevicesReport = function () {
        startLoading("Loading devices report...");

        $http.get("/api/reports/GetDevicesReport", {
            params: {
                categoryId: $scope.devicesReportFilters.categoryId || null,
                deviceId: $scope.devicesReportFilters.deviceId || null,
                phoneStatus: $scope.devicesReportFilters.phoneStatus || null
            }
        }).then(function (response) {
            $scope.devicesReport = response.data;
        }, function (error) {
            console.log(error);
            showError(error, "Error while loading devices report");
        }).finally(function () {
            stopLoading();
        });
    };

    $scope.exportDevicesReportWord = function () {
        var url = "/api/reports/ExportDevicesReportWord";

        var queryParams = [];

        if ($scope.devicesReportFilters.categoryId) {
            queryParams.push("categoryId=" + $scope.devicesReportFilters.categoryId);
        }

        if ($scope.devicesReportFilters.deviceId) {
            queryParams.push("deviceId=" + $scope.devicesReportFilters.deviceId);
        }

        if ($scope.devicesReportFilters.phoneStatus) {
            queryParams.push("phoneStatus=" + $scope.devicesReportFilters.phoneStatus);
        }

        if (queryParams.length > 0) {
            url += "?" + queryParams.join("&");
        }

        window.location.href = url;
    };

    $scope.exportDevicesReportPdf = function () {
        $scope.printMode = "devices";

        setTimeout(function () {
            window.print();
            $scope.printMode = "";
            $scope.$applyAsync();
        }, 300);
    };

    $scope.openDevicesReportModal = function () {
        var modalInstance = $uibModal.open({
            templateUrl: "generateDevicesReportModal.html",
            controller: "GenerateReportModalController"
        });

        modalInstance.result.then(function (format) {
            if (format === "word") {
                $scope.exportDevicesReportWord();
            }

            if (format === "pdf") {
                $scope.exportDevicesReportPdf();
            }
        });
    };

    $scope.loadReportClients = function () {
        return $http.get("/api/clients/GetAllClients")
            .then(function (response) {
                $scope.reportClients = response.data;
            }, function (error) {
                console.log(error);
                showError(error, "Error while loading clients");
            });
    };

    $scope.loadReportPhoneNumbers = function () {
        return $http.get("/api/phoneNumbers/GetAllPhoneNumbers")
            .then(function (response) {
                $scope.reportPhoneNumbers = response.data;
            }, function (error) {
                console.log(error);
                showError(error, "Error while loading phone numbers");
            });
    };

    $scope.loadReservationsReport = function () {
        startLoading("Loading reservations report...");

        $http.get("/api/phoneNumberReservations/GetFilteredReservations", {
            params: {
                clientId: $scope.reservationsReportFilters.clientId || null,
                phoneNumberId: $scope.reservationsReportFilters.phoneNumberId || null,
                status: $scope.reservationsReportFilters.status || null
            }
        }).then(function (response) {
            $scope.reservationsReport = response.data;
        }, function (error) {
            console.log(error);
            showError(error, "Error while loading reservations report");
        }).finally(function () {
            stopLoading();
        });
    };

    $scope.exportReservationsReportWord = function () {
        var url = "/api/phoneNumberReservations/ExportReservationsWord";

        var queryParams = [];

        if ($scope.reservationsReportFilters.clientId) {
            queryParams.push("clientId=" + $scope.reservationsReportFilters.clientId);
        }

        if ($scope.reservationsReportFilters.phoneNumberId) {
            queryParams.push("phoneNumberId=" + $scope.reservationsReportFilters.phoneNumberId);
        }

        if ($scope.reservationsReportFilters.status) {
            queryParams.push("status=" + $scope.reservationsReportFilters.status);
        }

        if (queryParams.length > 0) {
            url += "?" + queryParams.join("&");
        }

        window.location.href = url;
    };

    $scope.exportReservationsReportPdf = function () {
        $scope.printMode = "reservations";

        setTimeout(function () {
            window.print();
            $scope.printMode = "";
            $scope.$applyAsync();
        }, 300);
    };

    $scope.openReservationsReportModal = function () {
        var modalInstance = $uibModal.open({
            templateUrl: "generateReservationsReportModal.html",
            controller: "GenerateReportModalController"
        });

        modalInstance.result.then(function (format) {
            if (format === "word") {
                $scope.exportReservationsReportWord();
            }

            if (format === "pdf") {
                $scope.exportReservationsReportPdf();
            }
        });
    };

    $scope.loadSummary();
    $scope.loadReportsOverview();
});

app.controller("HomeItemsController", function ($scope, $http, $timeout) {

    $scope.homeItems = [];
    $scope.isAdmin = false;
    $scope.isLoading = false;
    $scope.loadingMessage = "Loading home page...";

    $scope.toast = {
        show: false,
        type: "success",
        title: "",
        message: ""
    };

    function showToast(type, title, message) {
        $scope.toast = {
            show: true,
            type: type,
            title: title,
            message: message
        };

        $timeout(function () {
            $scope.toast.show = false;
        }, 2500);
    }

    $scope.loadHomeItems = function () {
        $scope.isLoading = true;

        $http.get("/api/homeItems/GetHomeItems")
            .then(function (response) {
                $scope.isAdmin = response.data.isAdmin;
                $scope.homeItems = response.data.items;
            })
            .catch(function () {
                showToast("error", "Error", "Failed to load home items.");
            })
            .finally(function () {
                $scope.isLoading = false;
            });
    };

    $scope.toggleVisibility = function (item, event) {
        event.preventDefault();
        event.stopPropagation();

        $http.post("/api/homeItems/ToggleVisibility/" + item.id)
            .then(function (response) {
                item.isVisible = response.data.isVisible;

                showToast(
                    "success",
                    "Updated",
                    response.data.message
                );
            })
            .catch(function () {
                showToast("error", "Error", "Only admin can update visibility.");
            });
    };

    $scope.loadHomeItems();
});


app.controller("CentralImportResultModalController", function ($scope, $uibModalInstance, importResult) {

    $scope.importResult = importResult;

    $scope.cancel = function () {
        $uibModalInstance.dismiss("cancel");
    };
});

app.controller("CentralImportHistoryModalController", function ($scope, $http, $uibModal, $uibModalInstance) {

    $scope.importHistories = [];
    $scope.selectedHistory = null;
    $scope.selectedRows = [];
    $scope.errorMessage = "";
    $scope.successMessage = "";

    function showModalError(message) {
        $scope.errorMessage = message;
        $scope.successMessage = "";
    }

    function showModalSuccess(message) {
        $scope.successMessage = message;
        $scope.errorMessage = "";
    }

    $scope.loadHistory = function () {
        $http.get("/api/importHistory/GetAll")
            .then(function (response) {
                $scope.importHistories = response.data;

                if ($scope.importHistories.length > 0) {
                    $scope.selectHistory($scope.importHistories[0]);
                } else {
                    $scope.selectedHistory = null;
                    $scope.selectedRows = [];
                }
            }, function (error) {
                console.log(error);
                showModalError("Error while loading import history");
            });
    };

    $scope.selectHistory = function (history) {
        $scope.selectedHistory = history;
        $scope.selectedRows = [];

        $http.get("/api/importHistory/GetDetails/" + history.id)
            .then(function (response) {
                $scope.selectedRows = response.data;
            }, function (error) {
                console.log(error);
                showModalError("Error while loading import details");
            });
    };

    $scope.clearHistory = function () {

        var confirmModal = $uibModal.open({
            templateUrl: "clearCentralImportHistoryConfirmModal.html",
            controller: "ClearCentralImportHistoryConfirmController",
            windowClass: "delete-modal-window",
            backdrop: "static"
        });

        confirmModal.result.then(function () {

            $http.delete("/api/importHistory/ClearAll")
                .then(function () {
                    $scope.importHistories = [];
                    $scope.selectedHistory = null;
                    $scope.selectedRows = [];

                    showModalSuccess("Import history cleared successfully");
                }, function (error) {
                    console.log(error);
                    showModalError("Error while clearing import history");
                });

        });
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss("cancel");
    };

    $scope.loadHistory();
});

app.controller("ClearCentralImportHistoryConfirmController", function ($scope, $uibModalInstance) {

    $scope.confirmClear = function () {
        $uibModalInstance.close();
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss("cancel");
    };
});