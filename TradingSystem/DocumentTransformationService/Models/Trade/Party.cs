namespace DocumentTransformationService.Models.Trade;

/// <summary>
/// 🏢 Участник сделки (контрагент)
/// </summary>
public class Party {
    public string PartyId { get; set; } = string.Empty;
    public string PartyName { get; set; } = string.Empty;
    public string PartyRole { get; set; } = string.Empty; // Payer, Receiver
}