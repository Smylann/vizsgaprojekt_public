name=README.md url=https://github.com/Smylann/vizsgaprojekt_public

# Reddit 2 – A Modern Social News Platform

> A fully-featured social news platform where users can share content, discuss topics by category, vote on posts, and moderate community content. **Available on Web, iOS, Android, and Desktop.**

---

## 🎯 What is Reddit 2?

**Reddit 2** is a complete social news and discussion platform inspired by Reddit. Users create accounts, post content to categorized forums, engage with others through comments, and collectively determine what's important through upvoting and downvoting. Moderators and admins maintain community standards through content management and user moderation tools.

Built with a modern tech stack, Reddit 2 is **available across all major platforms**—use it on your phone, tablet, desktop, or web browser with a seamless, native experience on each platform.

---

## 📱 Multi-Platform Availability

### 🌐 Web Application
- Full-featured responsive web interface
- Works in all modern browsers
- Perfect for desktop and mobile browsers

### 📲 Native Mobile Apps
- **iOS** – Native app for iPhone and iPad
- **Android** – Native app for Android phones and tablets
- **Desktop** – Standalone applications for Windows and macOS
- Built with **.NET MAUI** – Single codebase, native performance on every platform

All platforms share the same backend API and offer identical functionality with platform-optimized user interfaces.

---

## 🌟 Core Features

### 👤 User Management
- **Registration & Authentication** – Create accounts with username, email, and password
- **Secure Login** – Cookie-based session authentication with SHA-256 password hashing
- **User Profiles** – View user information and published content
- **Account Settings** – Change passwords with real-time strength validation
- **Admin Roles** – Designate moderators and administrators with elevated permissions

### 📝 Content Creation & Discovery
- **Create Posts** – Users can publish posts with title, body content, and optional images
- **Categorize Content** – Organize posts by categories (manageable by admins)
- **Search & Filter** – Find posts by title, user, or category
- **Browse Feed** – Paginated main feed showing newest posts with filtering options
- **View Details** – Expand any post to see full content, comments, and metadata

### 🗣️ Community Engagement
- **Comment System** – Users can comment on posts for discussion
- **Upvote/Downvote** – Community voting determines post visibility and ranking
- **Save Posts** – Users can bookmark posts they want to revisit
- **View Post Stats** – See vote counts and comment threads at a glance

### 🛡️ Moderation & Reports
- **Content Reporting** – Users can report inappropriate posts with reasons
- **Admin Dashboard** – Comprehensive control panel to manage:
  - All users (rename, assign admin roles)
  - All posts (delete, view details)
  - All comments (browse and moderate)
  - Categories (create, delete, manage)
  - Reports (view open/closed reports with action history)
- **Report Tracking** – Reports can be marked as "Open", "Closed (not deleted)", or "Closed (deleted)"

### 🎨 User Experience
- **Platform-Optimized UI** – Native look and feel on iOS, Android, Desktop, and Web
- **Dark/Light Theme** – Toggle between Catppuccin-inspired dark and light modes
- **Real-time Validation** – Immediate feedback on form inputs (passwords, search, etc.)
- **Toast Notifications** – Non-intrusive feedback for actions and errors
- **Intuitive Navigation** – Sidebar categories, main feed, and user settings

---

## 🏗️ Technical Architecture

### Backend (C# / ASP.NET Core)
The backend runs as a RESTful API on **port 3070** with Swagger documentation:

**User Controller** – Authentication and account management
- Registration
- Login with session cookies
- Password modification
- User role management
- Get current user info

**News Controller** – Content management and discovery
- Post feed with pagination and category filtering
- Post search by title and category
- User search
- Comment retrieval and management
- Voting system (upvote/downvote)
- Save/favorite posts
- Content reporting

**Admin Controller** – Moderation tools
- Retrieve all users, posts, comments, categories
- Manage user roles (promote/demote admins)
- Delete posts and comments
- Create and delete categories
- Review and manage content reports

### Frontend – Multi-Platform Clients

#### Web (JavaScript / HTML / CSS)
Modular vanilla JavaScript with Bootstrap 5:
- Authentication pages (login & registration)
- Main feed with dynamic category filtering
- Post view with nested comments
- Create post modal
- Admin panel with tabbed moderation interface
- User settings and profile management

#### Mobile & Desktop (.NET MAUI / C#)
Single codebase compiled to native apps:
- **MobileVersion.Android** – Android native app
- **MobileVersion.iOS** – iOS native app
- **MobileVersion.Desktop** – Windows/macOS desktop app
- **MobileVersion.Browser** – Web-based deployment option

All MAUI projects share core logic while providing platform-specific UI optimizations.

### Database
Relational database schema supporting:
- Users (with roles: User/Admin)
- Posts (with categories, metadata, images)
- Comments (nested under posts)
- Votes (user-post relationships for upvote/downvote)
- Reports (content moderation tracking)
- Categories (for content organization)

---

## 🚀 How to Use

### For End Users

1. **Choose Your Platform** – Download the app for iOS/Android/Desktop or use the web version
2. **Register** – Create an account with username, email, and password
3. **Browse** – View posts from the main feed, filtered by category or search
4. **Engage** – Vote on posts, comment on discussions, save favorites
5. **Report** – Flag inappropriate content for moderation
6. **Manage** – Update your password in settings

### For Administrators

1. **Access Admin Panel** – Navigate from settings (admin-only)
2. **Manage Users** – Rename users, promote/demote admins
3. **Moderate Posts** – Delete inappropriate content
4. **Manage Comments** – Review and remove comments
5. **Handle Reports** – View reported content and take action
6. **Organize Categories** – Create categories for new topics

---

## 📋 Content Moderation Workflow

1. **User Reports Content** – Via "Report" button on any post
2. **Report Tracked** – Report status set to "Open"
3. **Admin Reviews** – Admin panel shows all open reports with reasons
4. **Action Taken** – Admin deletes post or closes report
5. **Status Updated** – Report marked "Closed (deleted)" or "Closed (not deleted)"

---

## 🔧 Deployment

### Web
Backend API configured for **0.0.0.0:3070**, accessible via: