using MobileVersion.Dtos;
using MobileVersion.ViewModels;

namespace MobileVersion.Messages;

public record GoBackMessage();
public record NavigateToPostMessage(PostVM PostVM);
public record NavigateToUserMessage(DisplayAllUserDTO User);
public record NavigateToOwnPostsMessage(OwnPostsVM ownpostsvm);
public record NavigateToOwnCommentsMessage(OwnCommentsVM owncommentsvm);
public record NavigateToLikedPostsMessage(LikedPostsVM likedpostsvm);
public record NavigateToDislikedPostsMessage(DislikedPostsVM dislikedpostsvm);
public record NavigateToFavoritesMessage(FavoritePostsVM favoritepostsvm);
public record NavigateToSettingsMessage(SettingsVM settingsvm);
public record NavigateToAboutUsMessage();
public record NavigateToLoginMessage(LoginVM loginvm);
public record NavigateToCreatePostMessage(CreatePostVM createvm);



