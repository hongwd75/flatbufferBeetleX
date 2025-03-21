﻿using Game.Logic.Inventory;

namespace Game.Logic.Currencys;

public class Wallet
    {
        private readonly Dictionary<Currency, long> balances = new Dictionary<Currency, long>();
        private GamePlayer owner;

        public Wallet() { }

        public Wallet(GamePlayer owner)
        {
            this.owner = owner;
        }

        public Money GetBalance(Currency currency)
        {
            if (currency.IsItemCurrency)
            {
                lock (owner.Inventory)
                {
                    return currency.Mint(owner.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack)
                        .Where(i => currency.IsSameCurrencyItem(i.Template))
                        .Aggregate(0, (acc, i) => acc + i.Count));
                }
            }
            
            lock (balances)
            {
                balances.TryGetValue(currency, out var balance);
                return currency.Mint(balance);
            }
        }

        public void AddMoney(Money money)
        {
            if (money.Currency.IsItemCurrency)
            {
                throw new ArgumentException("You cannot add money of type ItemCurrency.");
            }
            lock (balances)
            {
                var oldBalance = GetBalance(money.Currency).Amount;
                var newBalance = oldBalance + money.Amount;
                SetBalance(money.Currency.Mint(newBalance));
                SaveToDatabase();
            }
        }

        public bool RemoveMoney(Money money)
        {
            var currency = money.Currency;
            if (currency.IsItemCurrency)
            {
                lock (owner.Inventory)
                {
                    if (GetBalance(currency).Amount < money.Amount) return false;

                    var validCurrencyItemsInventory = owner.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack)
                        .Where(i => currency.IsSameCurrencyItem(i.Template));
                    var remainingDue = (int)money.Amount;
                    foreach (var currencyItem in validCurrencyItemsInventory)
                    {
                        if (currencyItem.Count >= remainingDue)
                        {
                            owner.Inventory.RemoveCountFromStack(currencyItem, remainingDue);
                            break;
                        }
                        else
                        {
                            remainingDue -= currencyItem.Count;
                            owner.Inventory.RemoveItem(currencyItem);
                        }
                    }
                    return true;
                }
            }
            lock (balances)
            {
                var oldBalance = GetBalance(money.Currency).Amount;
                if (oldBalance < money.Amount) return false;
                var newBalance = oldBalance - money.Amount;
                SetBalance(money.Currency.Mint(newBalance));
                SaveToDatabase();
                return true;
            }
        }

        /// <remarks>
        /// NOTE: This is going to be declared private in the future.
        /// </remarks>
        public void SetBalance(Money money)
        {
            lock (balances)
            {
                if (money.Amount == 0) balances.Remove(money.Currency);
                else balances[money.Currency] = money.Amount;
                UpdateCurrencyStatus(money.Currency);
            }
        }

        public void InitializeFromDatabase()
        {
            var dbCharacter = owner.DBCharacter;
            var initialCopperBalance = Game.Logic.Utils.Money.GetMoney(dbCharacter.Mithril, dbCharacter.Platinum, dbCharacter.Gold, dbCharacter.Silver, dbCharacter.Copper);
            SetBalance(Currency.Copper.Mint(initialCopperBalance));
            var initialBountyPoints = dbCharacter.BountyPoints;
            SetBalance(Currency.BountyPoints.Mint(initialBountyPoints));
        }

        public void SaveToDatabase()
        {
            if (owner == null || owner.DBCharacter == null) return;
            var dbCharacter = owner.DBCharacter;

            var copperBalance = GetBalance(Currency.Copper).Amount;
            dbCharacter.Copper = Game.Logic.Utils.Money.GetCopper(copperBalance);
            dbCharacter.Silver = Game.Logic.Utils.Money.GetSilver(copperBalance);
            dbCharacter.Gold = Game.Logic.Utils.Money.GetGold(copperBalance);
            dbCharacter.Platinum = Game.Logic.Utils.Money.GetPlatinum(copperBalance);
            dbCharacter.Mithril = Game.Logic.Utils.Money.GetMithril(copperBalance);
            dbCharacter.BountyPoints = GetBalance(Currency.BountyPoints).Amount;
        }

        private void UpdateCurrencyStatus(Currency currency)
        {
            if (owner != null && owner.Out != null)
            {
                if (currency.Equals(Currency.Copper)) owner.Out.SendUpdateMoney();
                else if (currency.Equals(Currency.BountyPoints)) owner.Out.SendUpdatePoints();
            }
        }
    }