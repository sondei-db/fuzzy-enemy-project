using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using FLS;
using FLS.Rules;

public class FuzzyRoninController 
{




    public double Shield(int inDistanceFromTarget)
    {
        var chargeLevel = new LinguisticVariable("ChargeLevel");
        var low = chargeLevel.MembershipFunctions.AddTriangle("Low", -1, 0, 50);
        var moderate = chargeLevel.MembershipFunctions.AddTriangle("Moderate", 40, 50, 60);
        var high = chargeLevel.MembershipFunctions.AddTriangle("High", 50, 100, 120);

        var distanceFromTarget = new LinguisticVariable("DistanceFromTarget");
        var close = distanceFromTarget.MembershipFunctions.AddTriangle("Close", 0, 0, 300);
        var medium = distanceFromTarget.MembershipFunctions.AddTrapezoid("Medium", 0, 175, 300, 500);
        var far = distanceFromTarget.MembershipFunctions.AddTriangle("Far", 300, 750, 1500);

        IFuzzyEngine fuzzyEngine = new FuzzyEngineFactory().Default();

        var rule1 = Rule.If(distanceFromTarget.Is(far)).Then(chargeLevel.Is(high));
        var rule2 = Rule.If(distanceFromTarget.Is(medium)).Then(chargeLevel.Is(moderate));
        var rule3 = Rule.If(distanceFromTarget.Is(close)).Then(chargeLevel.Is(low));
        
        fuzzyEngine.Rules.Add(rule1, rule2, rule3);

        var result = fuzzyEngine.Defuzzify(new {distanceFromTarget = inDistanceFromTarget });


        return result;
    }

    public double ShieldChance(int inHealthPercentage)
    {
        var shieldChance = new LinguisticVariable("ShieldChance");
        var low = shieldChance.MembershipFunctions.AddTriangle("Low", -1, 0, 50);
        var moderate = shieldChance.MembershipFunctions.AddTriangle("Moderate", 40, 50, 60);
        var high = shieldChance.MembershipFunctions.AddTriangle("High", 50, 100, 120);

        var healthPercentage = new LinguisticVariable("HealthPercentage");
        var small = healthPercentage.MembershipFunctions.AddTriangle("Small", 0, 0, 25);
        var medium = healthPercentage.MembershipFunctions.AddTrapezoid("Medium", 0, 20, 50, 70);
        var big = healthPercentage.MembershipFunctions.AddTriangle("Big", 60, 100, 300);

        IFuzzyEngine fuzzyEngine = new FuzzyEngineFactory().Default();

        var rule1 = Rule.If(healthPercentage.Is(small)).Then(shieldChance.Is(high));
        var rule2 = Rule.If(healthPercentage.Is(medium)).Then(shieldChance.Is(moderate));
        var rule3 = Rule.If(healthPercentage.Is(big)).Then(shieldChance.Is(low));

        fuzzyEngine.Rules.Add(rule1, rule2, rule3);

        var result = fuzzyEngine.Defuzzify(new { healthPercentage = inHealthPercentage });

        return result;
    }


    public double ShootDesirability(int distance, int inShieldCharge, int inTakenDmg) // int takenDmg
    {
        var shootDesirability = new LinguisticVariable("ShootDesirability");
        var undesirable = shootDesirability.MembershipFunctions.AddTrapezoid("Undesirable", 0, 0, 20, 50);
        var desirable = shootDesirability.MembershipFunctions.AddTriangle("Desirable", 25, 50, 75);
        var veryDesirable = shootDesirability.MembershipFunctions.AddTrapezoid("VeryDesirable", 50, 75, 100, 100);

        var distanceFromTarget = new LinguisticVariable("DistanceFromTarget");
        var close = distanceFromTarget.MembershipFunctions.AddTrapezoid("Close", 0, 0, 0, 200);
        var medium = distanceFromTarget.MembershipFunctions.AddTriangle("Medium", 175, 300, 500);
        var far = distanceFromTarget.MembershipFunctions.AddTrapezoid("Far", 450, 500, 950, 10000);

        var shieldCharge = new LinguisticVariable("ShieldCharge");
        var low = shieldCharge.MembershipFunctions.AddTrapezoid("Low", 0, 0, 25, 50);
        var mid = shieldCharge.MembershipFunctions.AddTriangle("Mid", 40, 50, 60);
        var high = shieldCharge.MembershipFunctions.AddTrapezoid("High", 50, 75, 100, 100);

        var takenDamage = new LinguisticVariable("TakenDamage");
        var smallDmg = takenDamage.MembershipFunctions.AddTrapezoid("SmallDmg", 0, 0, 50, 200);
        var mediumDmg = takenDamage.MembershipFunctions.AddTriangle("MediumDmg", 175, 300, 525);
        var bigDmg = takenDamage.MembershipFunctions.AddTrapezoid("BigDmg", 300, 600, 600, 600);

        IFuzzyEngine fuzzyEngine = new FuzzyEngineFactory().Default();

        var rule1 = Rule.If(distanceFromTarget.Is(far).And(shieldCharge.Is(high))).Then(shootDesirability.Is(veryDesirable));
        var rule11 = Rule.If(distanceFromTarget.Is(far).And(shieldCharge.Is(mid))).Then(shootDesirability.Is(veryDesirable));
        var rule2 = Rule.If(distanceFromTarget.Is(medium).And(shieldCharge.Is(high))).Then(shootDesirability.Is(desirable));
        var rule3 = Rule.If(distanceFromTarget.Is(medium).And(shieldCharge.Is(mid))).Then(shootDesirability.Is(desirable));
        var rule4 = Rule.If(distanceFromTarget.Is(close)).Then(shootDesirability.Is(undesirable));
        var rule5 = Rule.If(distanceFromTarget.Is(far).And(takenDamage.Is(mediumDmg))).Then(shootDesirability.Is(desirable));
        var rule6 = Rule.If(distanceFromTarget.Is(far).And(takenDamage.Is(bigDmg))).Then(shootDesirability.Is(undesirable));
        var rule7 = Rule.If(distanceFromTarget.Is(far).And(takenDamage.Is(smallDmg))).Then(shootDesirability.Is(veryDesirable));
        var rule8 = Rule.If(distanceFromTarget.Is(medium).And(takenDamage.Is(smallDmg))).Then(shootDesirability.Is(desirable));

        fuzzyEngine.Rules.Add(rule1, rule2, rule3, rule4, rule5, rule6, rule7, rule8, rule11);

        var result = fuzzyEngine.Defuzzify(new { distanceFromTarget = distance, shieldCharge = inShieldCharge, takenDamage = inTakenDmg });

        return result;

    }

    public double MeleeDesirability(int distance, int shieldChargeRate, int damageTaken)
    {
        var meleeDesirability = new LinguisticVariable("MeleeDesirability");
        var undesirable = meleeDesirability.MembershipFunctions.AddTrapezoid("Undesirable", 0, 0, 20, 50);
        var desirable = meleeDesirability.MembershipFunctions.AddTriangle("Desirable", 25, 50, 75);
        var veryDesirable = meleeDesirability.MembershipFunctions.AddTrapezoid("VeryDesirable", 50, 75, 100, 100);

        var distanceFromTarget = new LinguisticVariable("DistanceFromTarget");
        var close = distanceFromTarget.MembershipFunctions.AddTrapezoid("Close", 0, 0, 0, 200);
        var medium = distanceFromTarget.MembershipFunctions.AddTriangle("Medium", 175, 300, 500);
        var far = distanceFromTarget.MembershipFunctions.AddTrapezoid("Far", 450, 500, 950, 10000);

        var shieldCharge = new LinguisticVariable("ShieldCharge");
        var low = shieldCharge.MembershipFunctions.AddTrapezoid("Low", 0, 0, 25, 50);
        var mid = shieldCharge.MembershipFunctions.AddTriangle("Mid", 40, 50, 60);
        var high = shieldCharge.MembershipFunctions.AddTrapezoid("High", 50, 75, 100, 100);


        var takenDamage = new LinguisticVariable("TakenDamage");
        var smallDmg = takenDamage.MembershipFunctions.AddTrapezoid("SmallDmg", 0, 0, 50, 200);
        var mediumDmg = takenDamage.MembershipFunctions.AddTriangle("MediumDmg", 175, 300, 525);
        var bigDmg = takenDamage.MembershipFunctions.AddTrapezoid("BigDmg", 300, 600, 600, 600);

        IFuzzyEngine fuzzyEngine = new FuzzyEngineFactory().Default();

        var rule1 = Rule.If(distanceFromTarget.Is(close).And(shieldCharge.Is(low))).Then(meleeDesirability.Is(veryDesirable));
        var rule2 = Rule.If(distanceFromTarget.Is(medium).And(shieldCharge.Is(low))).Then(meleeDesirability.Is(desirable));
        var rule3 = Rule.If(distanceFromTarget.Is(far)).Then(meleeDesirability.Is(undesirable));

        var rule4 = Rule.If(distanceFromTarget.Is(close).And(shieldCharge.Is(low))).Then(meleeDesirability.Is(desirable));

        var rule5 = Rule.If(distanceFromTarget.Is(close).And(takenDamage.Is(bigDmg))).Then(meleeDesirability.Is(undesirable));
        var rule6 = Rule.If(distanceFromTarget.Is(medium).And(takenDamage.Is(bigDmg))).Then(meleeDesirability.Is(undesirable));
        var rule7 = Rule.If(distanceFromTarget.Is(close).And(takenDamage.Is(mediumDmg))).Then(meleeDesirability.Is(desirable));
        var rule8 = Rule.If(distanceFromTarget.Is(medium).And(takenDamage.Is(mediumDmg))).Then(meleeDesirability.Is(desirable));
        var rule9 = Rule.If(distanceFromTarget.Is(close).And(takenDamage.Is(smallDmg))).Then(meleeDesirability.Is(desirable));
        var rule10 = Rule.If(distanceFromTarget.Is(medium).And(takenDamage.Is(smallDmg))).Then(meleeDesirability.Is(desirable));


        fuzzyEngine.Rules.Add(rule1, rule2, rule3, rule4, rule5, rule6, rule7, rule8, rule9, rule10);

        var result = fuzzyEngine.Defuzzify(new { distanceFromTarget = distance, shieldCharge = shieldChargeRate, takenDamage = damageTaken });

        return result;
    }

    public double PlantMineDesirability(int distance, int aggressiveness)
    {
        var desirability = new LinguisticVariable("Desirability");
        var undesirable = desirability.MembershipFunctions.AddTrapezoid("Undesirable", 0, 0, 20, 50);
        var desirable = desirability.MembershipFunctions.AddTriangle("Desirable", 25, 50, 75);
        var veryDesirable = desirability.MembershipFunctions.AddTrapezoid("VeryDesirable", 50, 75, 100, 100);

        var aggro = new LinguisticVariable("Aggro");
        var lowAggro = aggro.MembershipFunctions.AddTrapezoid("lowAggro", 0, 0, 25, 50);
        var midAggro = aggro.MembershipFunctions.AddTriangle("midAggro", 25, 50, 75);
        var highAggro = aggro.MembershipFunctions.AddTrapezoid("highAggro", 50, 75, 100, 100);

        var distanceFromTarget = new LinguisticVariable("DistanceFromTarget");
        var close = distanceFromTarget.MembershipFunctions.AddTrapezoid("Close", 0, 0, 0, 200);
        var medium = distanceFromTarget.MembershipFunctions.AddTriangle("Medium", 175, 300, 500);
        var far = distanceFromTarget.MembershipFunctions.AddTrapezoid("Far", 450, 500, 950, 10000);

        IFuzzyEngine fuzzyEngine = new FuzzyEngineFactory().Default();

        var rule1 = Rule.If(distanceFromTarget.Is(far).And(aggro.Is(lowAggro))).Then(desirability.Is(undesirable));
        var rule2 = Rule.If(distanceFromTarget.Is(far).And(aggro.Is(midAggro))).Then(desirability.Is(undesirable));
        var rule3 = Rule.If(distanceFromTarget.Is(far).And(aggro.Is(highAggro))).Then(desirability.Is(undesirable));

        var rule4 = Rule.If(distanceFromTarget.Is(medium).And(aggro.Is(lowAggro))).Then(desirability.Is(undesirable));
        var rule5 = Rule.If(distanceFromTarget.Is(medium).And(aggro.Is(midAggro))).Then(desirability.Is(desirable));
        var rule6 = Rule.If(distanceFromTarget.Is(medium).And(aggro.Is(highAggro))).Then(desirability.Is(veryDesirable));


        var rule7 = Rule.If(distanceFromTarget.Is(close).And(aggro.Is(lowAggro))).Then(desirability.Is(undesirable));
        var rule8 = Rule.If(distanceFromTarget.Is(close).And(aggro.Is(midAggro))).Then(desirability.Is(veryDesirable));
        var rule9 = Rule.If(distanceFromTarget.Is(close).And(aggro.Is(highAggro))).Then(desirability.Is(veryDesirable));

        fuzzyEngine.Rules.Add(rule1, rule2, rule3, rule4, rule5, rule6, rule7, rule8, rule9);


        var result = fuzzyEngine.Defuzzify(new { distanceFromTarget = distance, aggro = aggressiveness });


        return result;
    }


    public double FireRingDesirability(int distance, int aggressiveness)
    {
        var desirability = new LinguisticVariable("Desirability");
        var undesirable = desirability.MembershipFunctions.AddTrapezoid("Undesirable", 0, 0, 20, 50);
        var desirable = desirability.MembershipFunctions.AddTriangle("Desirable", 25, 50, 75);
        var veryDesirable = desirability.MembershipFunctions.AddTrapezoid("VeryDesirable", 50, 75, 100, 100);

        var aggro = new LinguisticVariable("Aggro");
        var lowAggro = aggro.MembershipFunctions.AddTrapezoid("lowAggro", 0, 0, 25, 50);
        var midAggro = aggro.MembershipFunctions.AddTriangle("midAggro", 25, 50, 75);
        var highAggro = aggro.MembershipFunctions.AddTrapezoid("highAggro", 50, 75, 100, 100);

        var distanceFromTarget = new LinguisticVariable("DistanceFromTarget");
        var close = distanceFromTarget.MembershipFunctions.AddTrapezoid("Close", 0, 0, 0, 200);
        var medium = distanceFromTarget.MembershipFunctions.AddTriangle("Medium", 175, 300, 500);
        var far = distanceFromTarget.MembershipFunctions.AddTrapezoid("Far", 450, 500, 950, 10000);

        IFuzzyEngine fuzzyEngine = new FuzzyEngineFactory().Default();


        var rule1 = Rule.If(distanceFromTarget.Is(far).And(aggro.Is(lowAggro))).Then(desirability.Is(undesirable));
        var rule2 = Rule.If(distanceFromTarget.Is(far).And(aggro.Is(midAggro))).Then(desirability.Is(veryDesirable));
        var rule3 = Rule.If(distanceFromTarget.Is(far).And(aggro.Is(highAggro))).Then(desirability.Is(veryDesirable));

        var rule4 = Rule.If(distanceFromTarget.Is(medium).And(aggro.Is(lowAggro))).Then(desirability.Is(undesirable));
        var rule5 = Rule.If(distanceFromTarget.Is(medium).And(aggro.Is(midAggro))).Then(desirability.Is(desirable));
        var rule6 = Rule.If(distanceFromTarget.Is(medium).And(aggro.Is(highAggro))).Then(desirability.Is(veryDesirable));


        var rule7 = Rule.If(distanceFromTarget.Is(close).And(aggro.Is(lowAggro))).Then(desirability.Is(undesirable));
        var rule8 = Rule.If(distanceFromTarget.Is(close).And(aggro.Is(midAggro))).Then(desirability.Is(undesirable));
        var rule9 = Rule.If(distanceFromTarget.Is(close).And(aggro.Is(highAggro))).Then(desirability.Is(desirable));

        fuzzyEngine.Rules.Add(rule1, rule2, rule3, rule4, rule5, rule6, rule7, rule8, rule9);

        var result = fuzzyEngine.Defuzzify(new { distanceFromTarget = distance, aggro = aggressiveness });

        return result;
    }


    public double Aggressiveness(int agentHealth, int damageTaken)
    {
        var health = new LinguisticVariable("Health");
        var lowHealth = health.MembershipFunctions.AddTrapezoid("LowHealth", 0, 0, 35, 45);
        var midHealth = health.MembershipFunctions.AddTriangle("MidHealth", 35, 50, 65);
        var highHealth = health.MembershipFunctions.AddTrapezoid("HighHealth", 60, 75, 100, 100);

        var takenDamage = new LinguisticVariable("TakenDamage");
        var smallDmg = takenDamage.MembershipFunctions.AddTrapezoid("SmallDmg", 0, 0, 50, 200);
        var mediumDmg = takenDamage.MembershipFunctions.AddTriangle("MediumDmg", 175, 300, 525);
        var bigDmg = takenDamage.MembershipFunctions.AddTrapezoid("BigDmg", 300, 600, 600, 600);


        var aggro = new LinguisticVariable("Aggro");
        var lowAggro = aggro.MembershipFunctions.AddTrapezoid("lowAggro", 0, 0, 25, 50);
        var midAggro = aggro.MembershipFunctions.AddTriangle("midAggro", 25, 50, 75);
        var highAggro = aggro.MembershipFunctions.AddTrapezoid("highAggro", 50, 75, 100, 100);

        IFuzzyEngine fuzzyEngine = new FuzzyEngineFactory().Default();


        var rule1 = Rule.If(health.Is(highHealth).And(takenDamage.Is(bigDmg))).Then(aggro.Is(midAggro));
        var rule2 = Rule.If(health.Is(highHealth).And(takenDamage.Is(mediumDmg))).Then(aggro.Is(lowAggro));
        var rule3 = Rule.If(health.Is(highHealth).And(takenDamage.Is(smallDmg))).Then(aggro.Is(lowAggro));

        var rule4 = Rule.If(health.Is(midHealth).And(takenDamage.Is(bigDmg))).Then(aggro.Is(highAggro));
        var rule5 = Rule.If(health.Is(midHealth).And(takenDamage.Is(mediumDmg))).Then(aggro.Is(midAggro));
        var rule6 = Rule.If(health.Is(midHealth).And(takenDamage.Is(smallDmg))).Then(aggro.Is(lowAggro));

        var rule7 = Rule.If(health.Is(lowHealth)).Then(aggro.Is(highAggro));

        fuzzyEngine.Rules.Add(rule1, rule2, rule3, rule4, rule5, rule6, rule7);

        var result = fuzzyEngine.Defuzzify(new { health = agentHealth, takenDamage = damageTaken });

        return result;
    }


    public double TeleportFar(int distance, int shield)
    {
        return 0.0;
    }


    public double TeleportClose(int distance, int shield)
    {
        return 0.0;
    }



    public double KeepDistance(double shotDesirabilityDegree)
    {
        var shootDesirability = new LinguisticVariable("ShootDesirability");
        var undesirable = shootDesirability.MembershipFunctions.AddTrapezoid("Undesirable", 0, 0, 20, 50);
        var desirable = shootDesirability.MembershipFunctions.AddTriangle("Desirable", 25, 50, 75);
        var veryDesirable = shootDesirability.MembershipFunctions.AddTrapezoid("VeryDesirable", 50, 75, 100, 100);

        var distanceFromTarget = new LinguisticVariable("DistanceFromTarget");
        var close = distanceFromTarget.MembershipFunctions.AddTrapezoid("Close", 0, 0, 0, 200);
        var medium = distanceFromTarget.MembershipFunctions.AddTriangle("Medium", 175, 300, 500);
        var far = distanceFromTarget.MembershipFunctions.AddTrapezoid("Far", 450, 500, 950, 1000);

        

        IFuzzyEngine fuzzyEngine = new FuzzyEngineFactory().Default();

        var rule1 = Rule.If(shootDesirability.Is(veryDesirable)).Then(distanceFromTarget.Is(far));
        var rule2 = Rule.If(shootDesirability.Is(desirable)).Then(distanceFromTarget.Is(far));
        //var rule2 = Rule.If(shootDesirability.Is(desirable)).Then(distanceFromTarget.Is(medium));
        //var rule2 = Rule.If(distanceFromTarget.Is(medium).And(_shieldCharge.Is(high))).Then(shootDesirability.Is(desirable));
        //var rule3 = Rule.If(distanceFromTarget.Is(medium).And(_shieldCharge.Is(medium))).Then(shootDesirability.Is(desirable));
        //var rule4 = Rule.If(distanceFromTarget.Is(close)).Then(shootDesirability.Is(undesirable));

        fuzzyEngine.Rules.Add(rule1, rule2);

        var result = fuzzyEngine.Defuzzify(new { shootDesirability = shotDesirabilityDegree });


        return result;


        
    }

    public double StayClose(int takenDmd, int shieldCharge, double meleeDesirability)
    {
        var _shootDesirability = new LinguisticVariable("ShootDesirability");
        var undesirable = _shootDesirability.MembershipFunctions.AddTrapezoid("Undesirable", 0, 0, 20, 50);
        var desirable = _shootDesirability.MembershipFunctions.AddTriangle("Desirable", 25, 50, 75);
        var veryDesirable = _shootDesirability.MembershipFunctions.AddTrapezoid("VeryDesirable", 50, 75, 100, 100);

        var distanceFromTarget = new LinguisticVariable("DistanceFromTarget");
        var close = distanceFromTarget.MembershipFunctions.AddTrapezoid("Close", 0, 0, 0, 200);
        var medium = distanceFromTarget.MembershipFunctions.AddTriangle("Medium", 175, 300, 500);
        var far = distanceFromTarget.MembershipFunctions.AddTrapezoid("Far", 450, 500, 950, 1000);

        IFuzzyEngine fuzzyEngine = new FuzzyEngineFactory().Default();

        var rule1 = Rule.If(_shootDesirability.Is(veryDesirable)).Then(distanceFromTarget.Is(far));
        var rule2 = Rule.If(_shootDesirability.Is(desirable)).Then(distanceFromTarget.Is(medium));
        


        return 0.0;
    }






    public double Speed(int something)
    {
        return 0.0;
    }


   















}
