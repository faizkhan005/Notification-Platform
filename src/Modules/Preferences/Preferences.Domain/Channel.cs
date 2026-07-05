namespace Preferences.Domain;

/// <summary>
/// Represents a recipient's opt-out status for a specific channel, within a tenant.
///
/// WHY does Preferences define its own Channel enum instead of referencing
/// Notifications.Domain.NotificationChannel?
///
/// Preferences must never depend on Notifications (per our module dependency
/// rules — Notifications depends on Preferences, not the other way around,
/// or we'd have a circular reference). Duplicating a small, stable enum
/// like Channel is a standard, accepted tradeoff in modular monoliths:
/// a few lines of duplication is far cheaper than a circular dependency
/// or an awkward shared "Channel" concept living in BuildingBlocks (which
/// would suggest it's a technical concern, when it's actually a domain concept
/// each module reasons about independently).
///
/// DEFAULT SEMANTICS: absence of a Preference record means "opted in."
/// This matches real-world compliance: transactional notifications don't
/// require explicit opt-in consent, but explicit opt-outs must always be honored.
/// A Preference row only ever gets created when someone opts OUT.
/// </summary>
public enum Channel
{
    Email = 0,
    Sms = 1,
    InApp = 2,
    Slack = 3,
    Webhook = 4
}
