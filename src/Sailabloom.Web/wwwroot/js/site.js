// Sailabloom checkout helpers (progressive enhancement — the server
// enforces every rule again, so nothing depends on this script).
document.addEventListener("DOMContentLoaded", function () {
    var paymentRadios = document.querySelectorAll('input[name="Input.PaymentMethod"]');
    if (!paymentRadios.length) {
        return;
    }

    var cardFields = document.getElementById("cardFields");
    var zelleInfo = document.getElementById("zelleInfo");
    var fulfillmentRadios = document.querySelectorAll('input[name="Input.Fulfillment"]');
    var summary = document.getElementById("orderSummary");
    var shippingLine = document.getElementById("shippingLine");
    var totalLine = document.getElementById("totalLine");
    var subtotal = summary ? parseFloat(summary.getAttribute("data-subtotal") || "0") : 0;

    function money(n) {
        return "$" + n.toFixed(2);
    }

    function refresh() {
        var payment = document.querySelector('input[name="Input.PaymentMethod"]:checked');
        var method = payment ? payment.value : "Card";

        if (cardFields) cardFields.style.display = method === "Card" ? "" : "none";
        if (zelleInfo) zelleInfo.style.display = method === "Zelle" ? "" : "none";

        // Cash means pickup at home: force Pickup and lock the other choices.
        fulfillmentRadios.forEach(function (radio) {
            if (method === "Cash") {
                radio.checked = radio.value === "Pickup";
                radio.disabled = radio.value !== "Pickup";
            } else {
                radio.disabled = false;
            }
        });

        var fulfillment = document.querySelector('input[name="Input.Fulfillment"]:checked');
        var shipping = fulfillment && fulfillment.value === "StandardShipping" ? 15 : 0;
        if (shippingLine) shippingLine.textContent = money(shipping);
        if (totalLine) totalLine.textContent = money(subtotal + shipping);
    }

    paymentRadios.forEach(function (radio) { radio.addEventListener("change", refresh); });
    fulfillmentRadios.forEach(function (radio) { radio.addEventListener("change", refresh); });
    refresh();
});
