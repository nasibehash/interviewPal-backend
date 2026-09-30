// هر روش پرداخت یک کلاس با همان رابط (charge) است
class CardGateway {
  charge(amount) {
    return { method: 'card', amount, fee: Math.round(amount * 0.01) };
  }
}
class WalletGateway {
  charge(amount) {
    return { method: 'wallet', amount, fee: 0 };
  }
}
class InstallmentGateway {
  charge(amount) {
    return { method: 'installment', amount, fee: Math.round(amount * 0.03) };
  }
}

// کارخانه: تنها جایی که «کدام کلاس» تصمیم گرفته می‌شود
const gateways = { card: CardGateway, wallet: WalletGateway, installment: InstallmentGateway };

function createGateway(method) {
  if (!Object.hasOwn(gateways, method)) throw new Error(`روش پرداخت ناشناخته: ${method}`);
  return new gateways[method]();
}

// کد پرداخت فقط از رابط استفاده می‌کند و نمی‌داند چه کلاسی پشتش است
function checkout(order) {
  const receipt = createGateway(order.method).charge(order.total);
  console.log(`${receipt.method}: ${receipt.amount} + کارمزد ${receipt.fee}`);
}

checkout({ method: 'card', total: 500_000 }); // card: 500000 + کارمزد 5000
checkout({ method: 'installment', total: 500_000 }); // installment: 500000 + کارمزد 15000
