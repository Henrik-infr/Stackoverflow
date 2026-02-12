// Stack Overflow Clone - JavaScript

(function() {
    'use strict';

    // Voting functionality
    document.addEventListener('click', function(e) {
        const voteBtn = e.target.closest('.vote-btn');
        if (voteBtn) {
            e.preventDefault();
            handleVote(voteBtn);
        }

        const bookmarkBtn = e.target.closest('.bookmark-btn');
        if (bookmarkBtn) {
            e.preventDefault();
            handleBookmark(bookmarkBtn);
        }

        const addCommentLink = e.target.closest('.add-comment-link');
        if (addCommentLink) {
            e.preventDefault();
            handleAddComment(addCommentLink);
        }
    });

    function handleVote(button) {
        const postId = button.dataset.postId;
        const voteType = button.dataset.voteType;
        const voteCell = button.closest('.vote-cell');
        const voteCount = voteCell.querySelector('.vote-count');

        // Toggle voted state
        const isVoted = button.classList.contains('voted');

        if (isVoted) {
            button.classList.remove('voted');
            // Decrement/increment count based on vote type
            if (voteType === 'up') {
                voteCount.textContent = parseInt(voteCount.textContent) - 1;
            } else {
                voteCount.textContent = parseInt(voteCount.textContent) + 1;
            }
        } else {
            // Remove other vote if present
            const otherBtn = voteCell.querySelector(voteType === 'up' ? '.downvote' : '.upvote');
            if (otherBtn.classList.contains('voted')) {
                otherBtn.classList.remove('voted');
                voteCount.textContent = parseInt(voteCount.textContent) + (voteType === 'up' ? 1 : -1);
            }

            button.classList.add('voted');
            if (voteType === 'up') {
                voteCount.textContent = parseInt(voteCount.textContent) + 1;
            } else {
                voteCount.textContent = parseInt(voteCount.textContent) - 1;
            }
        }

        // In a real app, send AJAX request to server
        // fetch(`/api/vote`, {
        //     method: 'POST',
        //     headers: { 'Content-Type': 'application/json' },
        //     body: JSON.stringify({ postId, voteType, remove: isVoted })
        // });
    }

    function handleBookmark(button) {
        const postId = button.dataset.postId;
        const isBookmarked = button.classList.contains('bookmarked');

        if (isBookmarked) {
            button.classList.remove('bookmarked');
            button.textContent = '☆';
        } else {
            button.classList.add('bookmarked');
            button.textContent = '★';
        }

        // In a real app, send AJAX request
    }

    function handleAddComment(link) {
        const postId = link.dataset.postId;
        const commentsSection = link.closest('.comments-section');

        // Check if form already exists
        let form = commentsSection.querySelector('.comment-form');
        if (form) {
            form.remove();
            return;
        }

        // Create comment form
        form = document.createElement('form');
        form.className = 'comment-form';
        form.innerHTML = `
            <textarea class="comment-input" placeholder="Enter your comment..." rows="2"></textarea>
            <div class="comment-form-actions">
                <button type="submit" class="btn btn-primary btn-small">Add Comment</button>
                <button type="button" class="btn btn-secondary btn-small cancel-comment">Cancel</button>
            </div>
        `;

        form.addEventListener('submit', function(e) {
            e.preventDefault();
            const text = form.querySelector('.comment-input').value.trim();
            if (text) {
                // In a real app, send AJAX request
                addCommentToList(commentsSection, text);
                form.remove();
            }
        });

        form.querySelector('.cancel-comment').addEventListener('click', function() {
            form.remove();
        });

        commentsSection.appendChild(form);
        form.querySelector('.comment-input').focus();
    }

    function addCommentToList(section, text) {
        let list = section.querySelector('.comments-list');
        if (!list) {
            list = document.createElement('ul');
            list.className = 'comments-list';
            section.insertBefore(list, section.querySelector('.add-comment-link'));
        }

        const li = document.createElement('li');
        li.className = 'comment';
        li.innerHTML = `
            <span class="comment-score"></span>
            <span class="comment-text">${escapeHtml(text)}</span>
            <span class="comment-user">– You</span>
            <span class="comment-date">just now</span>
        `;
        list.appendChild(li);
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    // Tag input enhancement
    const tagInput = document.getElementById('Tags');
    if (tagInput) {
        tagInput.addEventListener('input', function() {
            // Could add autocomplete functionality here
        });
    }

    // Search suggestions (placeholder for future implementation)
    const searchInput = document.querySelector('.search-input, .search-input-large');
    if (searchInput) {
        let timeout;
        searchInput.addEventListener('input', function() {
            clearTimeout(timeout);
            const query = this.value.trim();
            if (query.length >= 3) {
                timeout = setTimeout(function() {
                    // In a real app, fetch suggestions
                    // fetch(`/api/search/suggestions?q=${encodeURIComponent(query)}`)
                }, 300);
            }
        });
    }

    // Relative time formatting
    function formatRelativeTime(date) {
        const now = new Date();
        const diff = now - date;
        const seconds = Math.floor(diff / 1000);
        const minutes = Math.floor(seconds / 60);
        const hours = Math.floor(minutes / 60);
        const days = Math.floor(hours / 24);

        if (seconds < 60) return 'just now';
        if (minutes < 60) return `${minutes} min${minutes !== 1 ? 's' : ''} ago`;
        if (hours < 24) return `${hours} hour${hours !== 1 ? 's' : ''} ago`;
        if (days < 7) return `${days} day${days !== 1 ? 's' : ''} ago`;

        return date.toLocaleDateString();
    }

    // Code highlighting (placeholder - would use library like highlight.js)
    document.querySelectorAll('pre code').forEach(function(block) {
        // In a real app, use highlight.js or similar
        // hljs.highlightBlock(block);
    });

    // Smooth scroll to answers
    if (window.location.hash && window.location.hash.startsWith('#answer-')) {
        const target = document.querySelector(window.location.hash);
        if (target) {
            setTimeout(function() {
                target.scrollIntoView({ behavior: 'smooth', block: 'start' });
            }, 100);
        }
    }
})();
